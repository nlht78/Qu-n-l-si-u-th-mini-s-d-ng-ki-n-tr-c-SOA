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
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EditModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public Product Product { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>(); // Danh sách loại sản phẩm

        public async Task OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("ProductService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Lấy thông tin sản phẩm
            var productResponse = await client.GetAsync($"/products/{id}");
            if (productResponse.IsSuccessStatusCode)
            {
                var productContent = await productResponse.Content.ReadAsStringAsync();
                Product = JsonConvert.DeserializeObject<Product>(productContent);
            }

            // Lấy danh sách loại sản phẩm
            var categoryResponse = await client.GetAsync("/categories");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryContent = await categoryResponse.Content.ReadAsStringAsync();
                Categories = JsonConvert.DeserializeObject<List<Category>>(categoryContent);
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
