using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Orders
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EditModel(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient("OrderService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PutAsJsonAsync($"/orders/{Order.Id}", Order);
            if (response.IsSuccessStatusCode)
                return RedirectToPage("./Index");

            ModelState.AddModelError(string.Empty, "Không thể cập nhật đơn hàng.");
            return Page();
        }
    }
}
