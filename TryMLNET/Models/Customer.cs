namespace TryMLNET.Models
{
    public class Customer
    {
        public DateTime MonthlySales { get; set; }
        public string CustomerId { get; set; }
        public double SaleAmount { get; set; }
    }

    public class PredictionCustomerModel
    {
        public DateTime MonthlySales { get; set; }
        public string CustomerId { get; set; }
        public double SaleAmount { get; set; }
    }
}
