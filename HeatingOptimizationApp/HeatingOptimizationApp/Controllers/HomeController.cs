using Microsoft.AspNetCore.Mvc;
using HeatingOptimizationApp.Models;
using Google.OrTools.LinearSolver;
using ClosedXML.Excel;
using System.IO;

namespace HeatingOptimizationApp.Controllers
{
    public class HomeController : Controller
    {
        
        [HttpGet]
        public IActionResult Index()
        {
          
            var model = new HeatingViewModel();
            var defaultData = new FuelDataModel();

            model.Demand = defaultData.B;
            model.Reserves = defaultData.R;
            model.QMatrix = defaultData.Q;

        
            model.Result = SolveOptimization(defaultData);
            return View(model);
        }

        [HttpPost]
        public IActionResult Calculate(HeatingViewModel inputModel)
        {
           
            var data = new FuelDataModel
            {
                B = inputModel.Demand,
                R = inputModel.Reserves,
                Q = inputModel.QMatrix
            };

            inputModel.Result = SolveOptimization(data);

            return View("Index", inputModel);
        }

        
        private OptimizationResult SolveOptimization(FuelDataModel data)
        {
            Solver solver = Solver.CreateSolver("GLOP");
            if (solver == null) return new OptimizationResult { Status = "Ошибка инициализации Solver" };

            int numUnits = 4;
            int numFuels = 3;

            Variable[,] x = new Variable[numUnits, numFuels];
            for (int i = 0; i < numUnits; i++)
                for (int j = 0; j < numFuels; j++)
                    x[i, j] = solver.MakeNumVar(0.0, double.PositiveInfinity, $"x_{i}_{j}");

            Objective objective = solver.Objective();
            for (int i = 0; i < numUnits; i++)
                for (int j = 0; j < numFuels; j++)
                    objective.SetCoefficient(x[i, j], data.Q[i, j]);
            objective.SetMaximization();

            for (int i = 0; i < numUnits; i++)
            {
                Constraint constraint = solver.MakeConstraint(data.B[i], double.PositiveInfinity);
                for (int j = 0; j < numFuels; j++) constraint.SetCoefficient(x[i, j], 1);
            }

            for (int j = 0; j < numFuels; j++)
            {
                Constraint constraint = solver.MakeConstraint(0, data.R[j]);
                for (int i = 0; i < numUnits; i++) constraint.SetCoefficient(x[i, j], 1);
            }

            Solver.ResultStatus resultStatus = solver.Solve();
            OptimizationResult result = new OptimizationResult();

            if (resultStatus == Solver.ResultStatus.OPTIMAL)
            {
                result.Status = "ОПТИМАЛЬНО";
                result.MaxHeat = objective.Value();
                result.X = new double[numUnits, numFuels];
                for (int i = 0; i < numUnits; i++)
                    for (int j = 0; j < numFuels; j++)
                        result.X[i, j] = Math.Round(x[i, j].SolutionValue(), 2);
            }
            else result.Status = "НЕТ РЕШЕНИЯ (INFEASIBLE)";

            return result;
        }

        
        [HttpPost]
        public IActionResult ExportToExcel(HeatingViewModel inputModel)
        {
            var data = new FuelDataModel
            {
                B = inputModel.Demand,
                R = inputModel.Reserves,
                Q = inputModel.QMatrix
            };
            var result = SolveOptimization(data);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Optimal Heating");
                worksheet.Cell(1, 1).Value = "ОТЧЕТ О РАСЧЕТЕ ОПТИМИЗАЦИИ ОБОГРЕВА";
                worksheet.Cell(1, 1).Style.Font.Bold = true;

                worksheet.Cell(3, 1).Value = "РЕЗУЛЬТАТЫ ОПТИМАЛЬНОГО РАСПРЕДЕЛЕНИЯ (X_ij):";
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 3; j++)
                        worksheet.Cell(4 + i, 1 + j).Value = result.X?[i, j] ?? 0;

                worksheet.Cell(9, 1).Value = "Максимальное количество теплоты (Z_max), МДж:";
                worksheet.Cell(9, 3).Value = result.MaxHeat;
                worksheet.Cell(9, 3).Style.Font.Bold = true;
                worksheet.Cell(9, 3).Style.Font.FontColor = XLColor.Red;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "HeatingReport.xlsx");
                }
            }
        }
    }
}
