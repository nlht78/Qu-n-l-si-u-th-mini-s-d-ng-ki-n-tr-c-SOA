using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using quanlisieuthimn.Models;
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
