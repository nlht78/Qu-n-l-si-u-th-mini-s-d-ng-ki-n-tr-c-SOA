using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Reports
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public string Type { get; set; } // "Order" or "Product"

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient("ReportService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var endpoint = Type.ToLower() == "order" ? $"/reports/orders/{Id}" : $"/reports/products/{Id}";
            var response = await client.DeleteAsync(endpoint);

            if (response.IsSuccessStatusCode)
                return RedirectToPage("./Index");

            ModelState.AddModelError(string.Empty, "Không thể xóa báo cáo.");
            return Page();
        }
    }
}
