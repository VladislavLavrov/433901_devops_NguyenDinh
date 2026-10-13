using _12_Calculator.Models;
using Microsoft.EntityFrameworkCore;

namespace _12_Calculator.Data
{
    public class CalculatorContext : DbContext
    {
        public DbSet<DataInputVariant> DataInputVariants { get; set; }

        public CalculatorContext(DbContextOptions<CalculatorContext> options) : base(options)
        {
        }
    }
}
