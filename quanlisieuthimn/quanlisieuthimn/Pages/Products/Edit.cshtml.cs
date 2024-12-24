using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
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

            var jsonContent = new StringContent(JsonConvert.SerializeObject(Product), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/products/{id}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Products/index");
            }

            ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật sản phẩm.");
            return Page();
        }
    }
}
