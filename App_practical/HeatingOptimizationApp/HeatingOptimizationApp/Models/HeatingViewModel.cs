namespace HeatingOptimizationApp.Models
{
    public class HeatingViewModel
    {
        public double[] Demand { get; set; } = new double[4]; 
        public double[] Reserves { get; set; } = new double[3]; 
        public double[,] QMatrix { get; set; } = new double[4, 3]; 
        public OptimizationResult? Result { get; set; } 
    }
}
