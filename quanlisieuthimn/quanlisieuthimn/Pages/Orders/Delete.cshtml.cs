using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Orders
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public Order Order { get; set; }

        public async Task OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("OrderService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"/orders/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Order = JsonConvert.DeserializeObject<Order>(content);
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("OrderService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"/orders/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToPage("./Index");

            ModelState.AddModelError(string.Empty, "Không thể xóa đơn hàng.");
            return Page();
        }
    }
}
