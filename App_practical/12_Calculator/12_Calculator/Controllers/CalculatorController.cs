using _12_Calculator.Data;
using _12_Calculator.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace _12_Calculator.Controllers
{
    public enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    public class CalculatorController : Controller
    {
        private readonly CalculatorContext _context;

        public CalculatorController(CalculatorContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // LẤY DỮ LIỆU TỪ DB ĐỂ HIỂN THỊ LÊN BẢNG
            var data = _context.DataInputVariants.OrderByDescending(x => x.ID_DataInputVariant).ToList();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calculate(double num1, double num2, Operation operation)
        {
            double result = 0;
            switch (operation)
            {
                case Operation.Add:
                    result = num1 + num2;
                    break;
                case Operation.Subtract:
                    result = num1 - num2;
                    break;
                case Operation.Multiply:
                    result = num1 * num2;
                    break;
                case Operation.Divide:
                    result = num1 / num2;
                    break;
            }

            var dataInputVariant = new DataInputVariant
            {
                Operand_1 = num1.ToString(),
                Operand_2 = num2.ToString(),
                Type_operation = operation.ToString(),
                Result = result.ToString()
            };

            _context.DataInputVariants.Add(dataInputVariant);
            _context.SaveChanges();

            // Chuyển hướng về trang Index để load lại bảng
            return RedirectToAction(nameof(Index));
        }
    }
}
