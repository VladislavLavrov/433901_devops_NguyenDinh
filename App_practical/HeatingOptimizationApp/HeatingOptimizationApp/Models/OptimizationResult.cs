namespace HeatingOptimizationApp.Models
{
    public class OptimizationResult
    {
        public double[,]? X { get; set; } 
        public double MaxHeat { get; set; } 
        public string? Status { get; set; } 
    }
}