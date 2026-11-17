namespace HeatingOptimizationApp.Models
{
    public class FuelDataModel
    {
       
        public double[,] Q { get; set; } = new double[,] {
            { 45, 10, 29 },
            { 35, 33, 70 },
            { 12, 14, 15 },
            { 60, 25, 6 }
        };
       
        public double[] B { get; set; } = { 80, 90, 70, 60 };
        
        public double[] R { get; set; } = { 140, 160, 100 };
    }
}
