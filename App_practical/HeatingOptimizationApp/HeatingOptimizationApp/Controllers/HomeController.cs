using Microsoft.AspNetCore.Mvc;
using HeatingOptimizationApp.Models;
using Google.OrTools.LinearSolver;
using ClosedXML.Excel;
using System.IO;

namespace HeatingOptimizationApp.Controllers
{
    public class HomeController : Controller
    {
        // Действие для отображения главной страницы и запуска оптимизации
        public IActionResult Index()
        {
            var data = new FuelDataModel();
            var result = SolveOptimization(data);
            return View(result);
        }

        // Метод решения задачи оптимизации с помощью Google OR-Tools
        private OptimizationResult SolveOptimization(FuelDataModel data)
        {
            // Инициализация решателя (GLOP для линейного программирования)
            Solver solver = Solver.CreateSolver("GLOP");
            if (solver == null) return new OptimizationResult { Status = "Ошибка инициализации Solver" };

            int numUnits = 4; // 4 агрегата
            int numFuels = 3; // 3 вида топлива

            // 1. Создание переменных решения x[i,j] >= 0
            Variable[,] x = new Variable[numUnits, numFuels];
            for (int i = 0; i < numUnits; i++)
                for (int j = 0; j < numFuels; j++)
                    x[i, j] = solver.MakeNumVar(0.0, double.PositiveInfinity, $"x_{i}_{j}");

            // 2. Целевая функция: Максимизация общего количества теплоты
            Objective objective = solver.Objective();
            for (int i = 0; i < numUnits; i++)
                for (int j = 0; j < numFuels; j++)
                    objective.SetCoefficient(x[i, j], data.Q[i, j]);
            objective.SetMaximization();

            // 3. Ограничение по потребности агрегатов (Сумма X >= B)
            for (int i = 0; i < numUnits; i++)
            {
                Constraint constraint = solver.MakeConstraint(data.B[i], double.PositiveInfinity);
                for (int j = 0; j < numFuels; j++)
                    constraint.SetCoefficient(x[i, j], 1);
            }

            // 4. Ограничение по запасам топлива (Сумма X <= R)
            for (int j = 0; j < numFuels; j++)
            {
                Constraint constraint = solver.MakeConstraint(0, data.R[j]);
                for (int i = 0; i < numUnits; i++)
                    constraint.SetCoefficient(x[i, j], 1);
            }

            // 5. Решение задачи
            Solver.ResultStatus resultStatus = solver.Solve();
            OptimizationResult result = new OptimizationResult();

            if (resultStatus == Solver.ResultStatus.OPTIMAL)
            {
                result.Status = "ОПТИМАЛЬНО"; // Trạng thái tối ưu
                result.MaxHeat = objective.Value();
                result.X = new double[numUnits, numFuels];
                for (int i = 0; i < numUnits; i++)
                    for (int j = 0; j < numFuels; j++)
                        result.X[i, j] = Math.Round(x[i, j].SolutionValue(), 2); // Округление до 2 знаков
            }
            else result.Status = "НЕТ РЕШЕНИЯ (INFEASIBLE)";

            return result;
        }

        // Действие для экспорта отчета в Excel
        [HttpPost]
        public IActionResult ExportToExcel()
        {
            var data = new FuelDataModel();
            var result = SolveOptimization(data);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Optimal Heating");

                worksheet.Cell(1, 1).Value = "ОТЧЕТ О РАСЧЕТЕ ОПТИМИЗАЦИИ ОБОГРЕВА";
                worksheet.Cell(1, 1).Style.Font.Bold = true;

                // Запись результатов
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
