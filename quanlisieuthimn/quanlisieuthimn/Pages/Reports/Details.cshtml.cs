using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Reports
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public string Type { get; set; } // "Order" or "Product"

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public OrderReport OrderReport { get; set; }
        public ProductReport ProductReport { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("ReportService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if (Type.ToLower() == "order")
            {
                var response = await client.GetAsync($"/reports/orders/{Id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    OrderReport = JsonConvert.DeserializeObject<OrderReport>(content);
                }
                else
                {
                    return RedirectToPage("./Index");
                }
            }
            else if (Type.ToLower() == "product")
            {
                var response = await client.GetAsync($"/reports/products/{Id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ProductReport = JsonConvert.DeserializeObject<ProductReport>(content);
                }
                else
                {
                    return RedirectToPage("./Products");
                }
            }

            return Page();
        }
    }
}
