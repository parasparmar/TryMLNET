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
        public List<string> results = new List<string>();


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
                var qry = @"SELECT 
                            Datefromparts(YEAR(A.OrderDate),MONTH(A.OrderDate),1) AS MonthlySales
                            , A.CustomerID
                            , Round(SUM((B.UnitPrice * B.Quantity) * (1 - B.Discount)),2) AS SaleAmount
                            FROM dbo.Orders AS A 
                            INNER JOIN dbo.[Order Details] AS B ON B.OrderID = A.OrderID
                            WHERE 1=1
                            And CustomerID = 'ALFKI'
                            GROUP BY 
                            DATEFROMPARTS(YEAR(A.OrderDate), MONTH(A.OrderDate), 1)
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

            var outputSeries = new List<MLModel.ModelOutput>();
            var dates = new List<DateTime>();
            DateTime from = DateTime.Parse("1998-10-01");
            DateTime to = DateTime.Parse("2000-10-01");
            for (var dt = from; dt <= to; dt = dt.AddMonths(1))
            {
                dates.Add(dt);
            }

            foreach (var i in Customers)
            {
                foreach (var d in dates)
                {
                    var j = new MLModel.ModelInput()
                    {
                        MonthlySales = d,
                        CustomerID = i.CustomerId
                    };
                    outputSeries.Add(MLModel.Predict(j));
                }
            }

            foreach (var d in outputSeries)
            {
                Customers.Add(new Customer
                {
                    CustomerId = d.CustomerID,
                    MonthlySales =d.MonthlySales,
                    SaleAmount = d.SaleAmount
                });
                results.Add($"Predicted Monthly Sales : {d.CustomerID}: {d.MonthlySales} : Sales Amount : {d.Score}");
            }
        }
    }
}

