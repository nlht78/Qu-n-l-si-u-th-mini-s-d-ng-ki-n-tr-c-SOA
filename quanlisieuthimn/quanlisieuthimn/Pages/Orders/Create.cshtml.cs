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
        public string? CustomerEmail { get; set; } // Sử dụng nullable type


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Dữ liệu không hợp lệ.");
                return Page();
            }

            var client = _httpClientFactory.CreateClient("OrderService");
            string token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError(string.Empty, "Token không tồn tại hoặc đã hết hạn.");
                return Page();
            }

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var newOrder = new
            {
                Order = new
                {
                    CustomerName,
                    CustomerEmail = string.IsNullOrEmpty(CustomerEmail) ? null : CustomerEmail
                }
            };

            try
            {
                var response = await client.PostAsync(
                    "/orders",
                    new StringContent(JsonConvert.SerializeObject(newOrder), Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Đơn hàng đã được tạo thành công.";
                    return RedirectToPage("/Products/Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Lỗi từ API: {response.StatusCode} - {errorContent}");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi khi gọi API: {ex.Message}");
                return Page();
            }
        }

    }
}
