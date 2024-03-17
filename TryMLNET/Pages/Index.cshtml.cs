using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TryMLNET.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {


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
