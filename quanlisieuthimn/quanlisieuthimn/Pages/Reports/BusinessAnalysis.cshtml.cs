using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Reports
{
    public class BusinessAnalysisModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BusinessAnalysisModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public BusinessAnalysisResult Analysis { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("ReportService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/reports/business-analysis");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Analysis = JsonConvert.DeserializeObject<BusinessAnalysisResult>(jsonResponse);
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Không thể tải phân tích doanh thu kinh doanh.");
            return Page();
        }

        
    }
}
