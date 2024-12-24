using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Products
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Product Product { get; set; }

        public async Task OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("ProductService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"/products/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Product = JsonConvert.DeserializeObject<Product>(content);
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("ProductService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"/products/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Products/index");
            }

            ModelState.AddModelError(string.Empty, "Lỗi khi xóa sản phẩm.");
            return Page();
        }
    }
}
