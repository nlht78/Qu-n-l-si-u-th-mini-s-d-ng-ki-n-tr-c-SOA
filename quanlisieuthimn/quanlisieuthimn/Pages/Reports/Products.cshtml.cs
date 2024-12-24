using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Reports
{
    public class ProductsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<ProductReport> ProductReports { get; set; } = new List<ProductReport>();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("ReportService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/reports/products");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ProductReports = JsonConvert.DeserializeObject<List<ProductReport>>(content);
            }
        }
    }
}
