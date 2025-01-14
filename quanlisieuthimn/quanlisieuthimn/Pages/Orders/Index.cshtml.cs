using Microsoft.AspNetCore.Mvc.RazorPages;

using Newtonsoft.Json;
using quanlisieuthimn.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace quanlisieuthimn.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<Order> Orders { get; set; } = new List<Order>();
        public string GetStatusClass(string status)
        {
            return status.ToLower() switch
            {
                "pending" => "badge-pending",
                "completed" => "badge-completed",
                "cancelled" => "badge-cancelled",
                _ => "badge-secondary"
            };
        }
        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("OrderService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/orders");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Orders = JsonConvert.DeserializeObject<List<Order>>(content);
            }
        }
    }
}
