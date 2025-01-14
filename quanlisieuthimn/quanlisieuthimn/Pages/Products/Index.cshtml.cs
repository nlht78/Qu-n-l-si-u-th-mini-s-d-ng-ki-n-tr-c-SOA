
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
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<Product> Products { get; set; } = new List<Product>();
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public int? SelectedOrderId { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>(); // Danh sách loại sản phẩm
        public int? SelectedCategoryId { get; set; } // ID loại sản phẩm được chọn

        public decimal SelectedOrderTotal { get; set; }


        [BindProperty]
        public int? SelectedProductId { get; set; }

        public async Task OnGetAsync(int? orderId = null, int? categoryId = null)
        {
            var client = _httpClientFactory.CreateClient("OrderService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Lưu trạng thái đơn hàng và loại sản phẩm được chọn
            SelectedOrderId = orderId;
            SelectedCategoryId = categoryId;

            // Lấy danh sách loại sản phẩm
            var categoryClient = _httpClientFactory.CreateClient("ProductService");
            categoryClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var categoryResponse = await categoryClient.GetAsync("/categories");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryContent = await categoryResponse.Content.ReadAsStringAsync();
                Categories = JsonConvert.DeserializeObject<List<Category>>(categoryContent);
            }

            // Lấy danh sách sản phẩm (lọc theo loại sản phẩm nếu có)
            var productClient = _httpClientFactory.CreateClient("ProductService");
            productClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var productResponse = await productClient.GetAsync(categoryId.HasValue ? $"/products/bycategory/{categoryId.Value}" : "/products");
            if (productResponse.IsSuccessStatusCode)
            {
                var productContent = await productResponse.Content.ReadAsStringAsync();
                Products = JsonConvert.DeserializeObject<List<Product>>(productContent);
            }

            // Lấy danh sách đơn hàng với trạng thái pending
            var orderResponse = await client.GetAsync("/orders/pending");
            if (orderResponse.IsSuccessStatusCode)
            {
                var orderContent = await orderResponse.Content.ReadAsStringAsync();
                Orders = JsonConvert.DeserializeObject<List<Order>>(orderContent);
            }

            // Lấy chi tiết đơn hàng nếu có orderId
            if (orderId.HasValue)
            {
                var orderItemsResponse = await client.GetAsync($"/order_items?orderId={orderId.Value}");
                if (orderItemsResponse.IsSuccessStatusCode)
                {
                    var orderItemsContent = await orderItemsResponse.Content.ReadAsStringAsync();
                    OrderItems = JsonConvert.DeserializeObject<List<OrderItem>>(orderItemsContent);
                }

                // Lấy tổng tiền của đơn hàng
                var orderDetailResponse = await client.GetAsync($"/orders/{orderId.Value}");
                if (orderDetailResponse.IsSuccessStatusCode)
                {
                    var orderDetailContent = await orderDetailResponse.Content.ReadAsStringAsync();
                    var selectedOrder = JsonConvert.DeserializeObject<Order>(orderDetailContent);
                    SelectedOrderTotal = selectedOrder.TotalAmount;
                }
                else
                {
                    SelectedOrderTotal = 0; // Nếu không lấy được tổng tiền, đặt giá trị mặc định
                }
            }
            else
            {
                SelectedOrderId = null;
                SelectedOrderTotal = 0; // Nếu không có orderId, đặt giá trị mặc định
            }
        }


        public async Task<IActionResult> OnPostAddToOrderAsync(int? orderId)
        {
            if (!orderId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một đơn hàng trước khi thêm sản phẩm.";
                return RedirectToPage(new { orderId });
            }

            SelectedOrderId = orderId;

            if (!SelectedProductId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một sản phẩm để thêm.";
                return RedirectToPage(new { orderId });
            }

            // Tải lại danh sách sản phẩm để kiểm tra sản phẩm đã chọn
            var productClient = _httpClientFactory.CreateClient("ProductService");
            string token = HttpContext.Session.GetString("JWToken");
            productClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var productResponse = await productClient.GetAsync("/products");
            if (productResponse.IsSuccessStatusCode)
            {
                var productContent = await productResponse.Content.ReadAsStringAsync();
                Products = JsonConvert.DeserializeObject<List<Product>>(productContent);
            }

            var selectedProduct = Products.Find(p => p.Id == SelectedProductId.Value);
            if (selectedProduct == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm đã chọn.";
                return RedirectToPage(new { orderId });
            }

            var newItem = new
            {
                OrderId = SelectedOrderId.Value,
                ProductId = selectedProduct.Id,
                ProductName = selectedProduct.Name,
                Quantity = 1, // Giá trị mặc định
                UnitPrice = selectedProduct.Price
            };

            var client = _httpClientFactory.CreateClient("OrderService");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync("/order_items", new StringContent(JsonConvert.SerializeObject(newItem), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không thể thêm sản phẩm vào đơn hàng.";
                return RedirectToPage(new { orderId });
            }

            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào đơn hàng thành công.";
            return RedirectToPage(new { orderId });
        }


        public async Task<IActionResult> OnPostRemoveFromOrderAsync(int orderId, int productId)
        {
            if (orderId <= 0 || productId <= 0)
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                return RedirectToPage(new { orderId });
            }

            var client = _httpClientFactory.CreateClient("OrderService");
            string token = HttpContext.Session.GetString("JWToken");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Gửi yêu cầu xóa sản phẩm khỏi đơn hàng
            var response = await client.DeleteAsync($"/order_items/order/{orderId}/product/{productId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm khỏi đơn hàng.";
                return RedirectToPage(new { orderId });
            }

            TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi đơn hàng thành công.";
            return RedirectToPage(new { orderId });
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int? orderId, int? productId, int newQuantity)
        {
            if (!orderId.HasValue || !productId.HasValue || newQuantity < 1)
            {
                TempData["ErrorMessage"] = "Thông tin đơn hàng hoặc sản phẩm không hợp lệ.";
                return RedirectToPage(new { orderId });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("OrderService");
                string token = HttpContext.Session.GetString("JWToken");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var updatePayload = new { Quantity = newQuantity };

                var response = await client.PutAsJsonAsync($"/order_items/{orderId.Value}/product/{productId.Value}/quantity", updatePayload);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Không thể cập nhật số lượng sản phẩm: {errorContent}";
                    return RedirectToPage(new { orderId });
                }

                TempData["SuccessMessage"] = "Số lượng sản phẩm đã được cập nhật thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi cập nhật số lượng sản phẩm: {ex.Message}";
            }

            return RedirectToPage(new { orderId });
        }


        public async Task<IActionResult> OnPostUpdateOrderStatusAsync(int? orderId, string status)
        {
            if (!orderId.HasValue || string.IsNullOrEmpty(status))
            {
                TempData["ErrorMessage"] = "Thông tin đơn hàng hoặc trạng thái không hợp lệ.";
                return RedirectToPage(new { orderId });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("OrderService");
                string token = HttpContext.Session.GetString("JWToken");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var updatePayload = new { Status = status };

                var response = await client.PutAsJsonAsync($"/orders/{orderId.Value}", updatePayload);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Không thể cập nhật trạng thái đơn hàng: {errorContent}";
                    return RedirectToPage(new { orderId });
                }

                TempData["SuccessMessage"] = status == "completed"
                    ? "Đơn hàng đã được thanh toán."
                    : "Đơn hàng đã bị hủy.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi cập nhật trạng thái đơn hàng: {ex.Message}";
            }

            return RedirectToPage(new { orderId });
        }


        public async Task<IActionResult> OnPostUpdateOrderStatusAndCreateReportAsync(int? orderId, string status)
        {
            if (!orderId.HasValue || string.IsNullOrEmpty(status))
            {
                TempData["ErrorMessage"] = "Thông tin đơn hàng hoặc trạng thái không hợp lệ.";
                return RedirectToPage(new { orderId });
            }

            try
            {
                // Bước 1: Cập nhật trạng thái đơn hàng
                var orderServiceClient = _httpClientFactory.CreateClient("OrderService");
                string token = HttpContext.Session.GetString("JWToken");
                orderServiceClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var updatePayload = new { Status = status };

                var updateResponse = await orderServiceClient.PutAsJsonAsync($"/orders/{orderId.Value}", updatePayload);
                if (!updateResponse.IsSuccessStatusCode)
                {
                    var errorContent = await updateResponse.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Không thể cập nhật trạng thái đơn hàng: {errorContent}";
                    return RedirectToPage(new { orderId });
                }

                // Nếu trạng thái là "completed", tạo báo cáo
                if (status == "completed")
                {
                    // Bước 2: Lấy thông tin chi tiết đơn hàng
                    var orderItemsResponse = await orderServiceClient.GetAsync($"/order_items?orderId={orderId.Value}");
                    if (!orderItemsResponse.IsSuccessStatusCode)
                    {
                        TempData["ErrorMessage"] = "Không thể lấy chi tiết đơn hàng để tạo báo cáo.";
                        return RedirectToPage(new { orderId });
                    }

                    var orderItemsContent = await orderItemsResponse.Content.ReadAsStringAsync();
                    var orderItems = JsonConvert.DeserializeObject<List<OrderItem>>(orderItemsContent);

                    // Chuẩn bị payload cho báo cáo
                    var reportPayload = new
                    {
                        OrderId = orderId.Value,
                        Products = orderItems.Select(item => new
                        {
                            ProductId = item.ProductId,
                            TotalSold = item.Quantity,
                            Revenue = item.UnitPrice
                        }).ToList()
                    };

                    // Bước 3: Gửi yêu cầu tạo báo cáo
                    var reportServiceClient = _httpClientFactory.CreateClient("ReportService");
                    reportServiceClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var reportResponse = await reportServiceClient.PostAsJsonAsync("/reports/orders", reportPayload);
                    if (!reportResponse.IsSuccessStatusCode)
                    {
                        var errorContent = await reportResponse.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"Không thể tạo báo cáo: {errorContent}";
                        return RedirectToPage(new { orderId });
                    }

                    TempData["SuccessMessage"] = "Đơn hàng đã được thanh toán và báo cáo đã được tạo.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Đơn hàng đã bị hủy.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
            }

            return RedirectToPage(new { orderId });
        }






    }
}
