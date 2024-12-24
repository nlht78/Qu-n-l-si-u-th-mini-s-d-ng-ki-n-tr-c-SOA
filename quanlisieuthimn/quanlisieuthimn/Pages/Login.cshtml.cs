using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient("AuthService");

            var loginData = new { UserName = Username, Password = Password };
            var response = await client.PostAsJsonAsync("/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(content);

                if (result?.Token != null)
                {
                    HttpContext.Session.SetString("JWToken", (string)result.Token);
                    return RedirectToPage("/Products/index");
                }
                else
                {
                    ErrorMessage = "Không nhận được token từ API.";
                }
            }
            else
            {
                ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
            }

            return Page();
        }

    }
}
