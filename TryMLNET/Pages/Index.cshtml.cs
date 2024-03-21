using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using TryMLNET.Models;
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
                var qry = @"SELECT TOP (100) PERCENT CONVERT(Date, CONVERT(varchar, YEAR(A.OrderDate)) + '-' + CONVERT(varchar, MONTH(A.OrderDate)) + '-' + CONVERT(varchar, 1)) AS MonthlySales
                            , A.CustomerID
                            , SUM(B.UnitPrice * B.Quantity - B.Discount) AS SaleAmount
                            FROM dbo.Orders AS A 
                            INNER JOIN dbo.[Order Details] AS B ON B.OrderID = A.OrderID
                            GROUP BY CONVERT(Date, CONVERT(varchar, YEAR(A.OrderDate)) + '-' + CONVERT(varchar, MONTH(A.OrderDate)) + '-' + CONVERT(varchar, 1))
                            , A.CustomerID
                            order by CustomerID, MonthlySales";
                using (SqlCommand cmd = new SqlCommand(qry, cn))
                {
                    SqlDataReader r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        Customers.Add(new Customer
                        {
                            MonthlySales = ((DateTime)r.GetDateTime(0)).ToLocalTime(),
                            CustomerId = (string)r.GetSqlString(1),
                            SaleAmount = r.GetDouble(2)
                        });
                    }
                }
                if (!cn.State.Equals(ConnectionState.Closed))
                    cn.Close();
            }

            var sampleData = new MLModel.ModelInput()
            {
                MonthlySales = DateTime.Parse("01-10-1997 00:00:00"),
                CustomerID = @"ALFKI",
            };

            //Load model and predict output
            var result = MLModel.Predict(sampleData);

        }
    }
}

