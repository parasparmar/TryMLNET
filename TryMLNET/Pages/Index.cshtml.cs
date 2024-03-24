using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.ML;
using Models;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using TryMLNET.Models;
using static Plotly.NET.StyleParam;
using Customer = TryMLNET.Models.Customer;
namespace TryMLNET.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public List<Customer> Customers = new List<Customer>();
        public string MLResult = string.Empty;
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
                            WHERE 1=1
                            And CustomerID = 'ALFKI'
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

            MLContext mlContext = new MLContext();

            IDataView trainingData = mlContext.Data.LoadFromEnumerable(Customers);

            // 2. Specify data preparation and model training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "MonthlySales" })
                .Append(mlContext.Regression.Trainers.FastForest());
            

            // 3. Train model
            var model = pipeline.Fit(trainingData);


            // 4. Make a prediction
            var monthlySales = new Customer {
                MonthlySales = DateTime.Parse("01-10-1997 00:00:00"),
                CustomerId = @"ALFKI"
            };
            var saleAmount = mlContext.Model.CreatePredictionEngine<Customer, PredictionCustomerModel>(model).Predict(monthlySales);


            //Load model and predict output

            MLResult = $"Predicted Monthly Sales for {monthlySales.CustomerId}: {monthlySales.MonthlySales.ToLongDateString} sq ft= {saleAmount.SaleAmount:C}k"; // JsonSerializer.Serialize(saleAmount);

        }
    }
}

