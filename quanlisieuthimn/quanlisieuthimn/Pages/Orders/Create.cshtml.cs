using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string CustomerName { get; set; }
        [BindProperty]
        public string CustomerEmail { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient("OrderService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var newOrder = new { Order = new { CustomerName, CustomerEmail } };
            var response = await client.PostAsync("/orders", new StringContent(JsonConvert.SerializeObject(newOrder), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
                return RedirectToPage("./Index");

            ModelState.AddModelError(string.Empty, "Không thể tạo đơn hàng.");
            return Page();
        }
    }
}
