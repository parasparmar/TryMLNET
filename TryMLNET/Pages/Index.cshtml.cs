using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
namespace TryMLNET.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public List<Customer> Customers = new List<Customer>();

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;            
        }

        public void OnGet()
        {
            var cstr = "Server=Mainframe-007;Database=Northwind;Trusted_Connection=True;";            
            
            using (SqlConnection cn = new SqlConnection(cstr))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("Select * from vwMonthly_Sales_by_Customer", cn))
                {
                    SqlDataReader r = cmd.ExecuteReader();
                    while (r.Read())
                    {                        
                        Customers.Add(new Customer
                        {
                            MonthlySales = (DateTime)r.GetDateTime(0),
                            CustomerId = (string)r.GetSqlString(1),
                            SaleAmount = r.GetDouble(2)
                        });
                    }
                }
                if (!cn.State.Equals(ConnectionState.Closed))
                    cn.Close();
            }

            
            ////Load sample data
            //var sampleData = new MLModel.ModelInput()
            //{
            //    MonthlySales = DateTime.Parse("01-10-1997 00:00:00"),
            //    CustomerID = @"ALFKI",
            //};

            ////Load model and predict output
            //var result = MLModel.Predict(sampleData);

        }
    }
}
public class Customer
{
    public DateTime MonthlySales { get; set; }
    public string CustomerId { get; set; }
    public double SaleAmount { get; set; }
}
