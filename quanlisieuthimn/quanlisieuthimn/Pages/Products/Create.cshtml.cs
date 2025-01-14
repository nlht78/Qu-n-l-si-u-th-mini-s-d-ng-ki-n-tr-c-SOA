using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public Product Product { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>(); // Danh sách loại sản phẩm

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("ProductService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Lấy danh sách loại sản phẩm từ API
            var response = await client.GetAsync("/categories");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Categories = JsonConvert.DeserializeObject<List<Category>>(content);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient("ProductService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var jsonContent = new StringContent(JsonConvert.SerializeObject(Product), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/products", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Products/index");
            }

            ModelState.AddModelError(string.Empty, "Lỗi khi thêm sản phẩm.");
            return Page();
        }
    }
}
