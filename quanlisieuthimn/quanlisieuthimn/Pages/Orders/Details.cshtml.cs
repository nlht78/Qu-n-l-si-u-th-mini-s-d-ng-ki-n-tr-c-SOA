using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public Order Order { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("OrderService");

            try
            {
                string token = HttpContext.Session.GetString("JWToken");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"/orders/{Id}/details");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<OrderDetailsResponse>(jsonResponse);

                    Order = data.Order;
                    OrderItems = data.OrderItems;
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể tải thông tin đơn hàng.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
            }

            return RedirectToPage("./Index");
        }
    }

    
}
