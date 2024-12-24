using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<OrderReport> OrderReports { get; set; } = new List<OrderReport>();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("ReportService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/reports/orders");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                OrderReports = JsonConvert.DeserializeObject<List<OrderReport>>(content);
            }
        }
    }
}
