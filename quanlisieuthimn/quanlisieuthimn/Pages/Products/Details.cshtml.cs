using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Product Product { get; set; }
        public string CategoryName { get; set; } // Tên loại sản phẩm

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

                // Lấy thông tin loại sản phẩm nếu CategoryId hợp lệ
                if (Product.CategoryId > 0)
                {
                    var categoryResponse = await client.GetAsync($"/categories/{Product.CategoryId}");
                    if (categoryResponse.IsSuccessStatusCode)
                    {
                        var categoryContent = await categoryResponse.Content.ReadAsStringAsync();
                        var category = JsonConvert.DeserializeObject<Category>(categoryContent);
                        CategoryName = category.Name;
                    }
                }
                else
                {
                    CategoryName = "Không xác định";
                }
            }
        }
    }
}
