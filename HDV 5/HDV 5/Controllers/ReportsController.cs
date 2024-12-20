// ReportsController.cs - Full configuration of requested APIs for reports service

using HDV_5.Models;
using HDV_5.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace HDV_5.Controllers
{
    [JwtAuthentication] // Require authentication via JWT
    public class ReportsController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ReportDBConnection"].ConnectionString;

        private List<Product> GetProducts(HttpRequestMessage request)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44361/"); // URL dịch vụ quản lý sản phẩm
                try
                {
                    // Lấy Authorization Header từ request gốc
                    if (request.Headers.Authorization == null || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
                    {
                        throw new Exception("Authorization header is missing or invalid.");
                    }

                    var jwtToken = request.Headers.Authorization.Parameter;

                    // Thêm Authorization Key vào Header
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = client.GetAsync("products").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content.ReadAsAsync<List<Product>>().Result;
                    }
                    else
                    {
                        var error = response.Content.ReadAsStringAsync().Result;
                        throw new Exception($"Failed to fetch products. Status: {response.StatusCode}, Error: {error}");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to fetch products from product service.", ex);
                }
            }
        }


        private List<Order> GetOrders(HttpRequestMessage request)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44362/"); // URL dịch vụ quản lý đơn hàng
                try
                {
                    // Lấy Authorization Header từ request gốc
                    if (request.Headers.Authorization == null || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
                    {
                        throw new Exception("Authorization header is missing or invalid.");
                    }

                    var jwtToken = request.Headers.Authorization.Parameter;

                    // Thêm Authorization Key vào Header
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = client.GetAsync("orders").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content.ReadAsAsync<List<Order>>().Result;
                    }
                    else
                    {
                        // Ghi nhật ký lỗi nếu phản hồi không thành công
                        var error = response.Content.ReadAsStringAsync().Result;
                        throw new Exception($"Failed to fetch orders. Status: {response.StatusCode}, Error: {error}");
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi kết nối hoặc lỗi khác
                    throw new Exception("Failed to fetch orders from order service.", ex);
                }
            }
        }




        private List<OrderItem> GetOrderItems(HttpRequestMessage request)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44362/"); // URL dịch vụ quản lý đơn hàng
                try
                {
                    // Lấy Authorization Header từ request gốc
                    if (request.Headers.Authorization == null || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
                    {
                        throw new Exception("Authorization header is missing or invalid.");
                    }

                    var jwtToken = request.Headers.Authorization.Parameter;

                    // Thêm Authorization Key vào Header
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = client.GetAsync("order_items").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content.ReadAsAsync<List<OrderItem>>().Result;
                    }
                    else
                    {
                        var error = response.Content.ReadAsStringAsync().Result;
                        throw new Exception($"Failed to fetch order items. Status: {response.StatusCode}, Error: {error}");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to fetch order items from order service.", ex);
                }
            }
        }





        // GET /reports/products
        [HttpGet]
        [Route("reports/products")]
        public IHttpActionResult GetProductReports()
        {
            var reports = new List<ProductReport>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM product_reports", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    reports.Add(new ProductReport
                    {
                        Id = (int)reader["id"],
                        OrderReportId = (int)reader["order_report_id"],
                        ProductId = (int)reader["product_id"],
                        TotalSold = (int)reader["total_sold"],
                        Revenue = (decimal)reader["revenue"],
                        Cost = (decimal)reader["cost"],
                        Profit = (decimal)reader["profit"]
                    });
                }
            }
            return Ok(reports);
        }

        // GET /reports/products/{id}
        [HttpGet]
        [Route("reports/products/{id:int}")]
        public IHttpActionResult GetProductReportById(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM product_reports WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var report = new ProductReport
                    {
                        Id = (int)reader["id"],
                        OrderReportId = (int)reader["order_report_id"],
                        ProductId = (int)reader["product_id"],
                        TotalSold = (int)reader["total_sold"],
                        Revenue = (decimal)reader["revenue"],
                        Cost = (decimal)reader["cost"],
                        Profit = (decimal)reader["profit"]
                    };
                    return Ok(report);
                }
                return NotFound();
            }
        }

        [HttpGet]
        [Route("test/orders")]
        public IHttpActionResult TestGetOrders()
        {
            try
            {
                // Truyền request gốc vào GetOrders
                var orders = GetOrders(Request);

                // Trả về danh sách đơn hàng dưới dạng JSON
                return Ok(orders);
            }
            catch (Exception ex)
            {
                // Nếu xảy ra lỗi, trả về thông tin lỗi
                return InternalServerError(ex);
            }
        }



        // POST /reports/products
        [HttpPost]
        [Route("reports/products")]
        public IHttpActionResult CreateProductReport(ProductReportRequest reportRequest)
        {
            try
            {
                // 1. Gọi API /products để lấy thông tin sản phẩm
                var products = GetProducts(Request);
                var product = products.FirstOrDefault(p => p.Id == reportRequest.ProductId);
                if (product == null)
                {
                    return NotFound(); // Không tìm thấy sản phẩm
                }

                // 2. Gọi API /order_items để lấy dữ liệu bán hàng cho sản phẩm
                var orderItems = GetOrderItems(Request);
                var productOrderItems = orderItems.Where(o => o.ProductId == reportRequest.ProductId);

                // 3. Tính toán TotalSold, Revenue và Profit
                var totalSold = productOrderItems.Sum(o => o.Quantity); // Tổng số lượng đã bán
                var revenue = totalSold * product.Price; // Tổng doanh thu
                var cost = reportRequest.Cost; // Chi phí sản phẩm từ client
                

                // 4. Lưu vào cơ sở dữ liệu
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "INSERT INTO product_reports (order_report_id, product_id, total_sold, revenue, cost) VALUES (@orderReportId, @productId, @totalSold, @revenue, @cost)",
                        connection
                    );
                    command.Parameters.AddWithValue("@orderReportId", reportRequest.OrderReportId);
                    command.Parameters.AddWithValue("@productId", reportRequest.ProductId);
                    command.Parameters.AddWithValue("@totalSold", totalSold);
                    command.Parameters.AddWithValue("@revenue", revenue);
                    command.Parameters.AddWithValue("@cost", cost);
                    

                    command.ExecuteNonQuery();
                }

                return Ok("Product report created successfully.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        // DELETE /reports/products/{id}
        [HttpDelete]
        [Route("reports/products/{id:int}")]
        public IHttpActionResult DeleteProductReport(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM product_reports WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
            return Ok("Product report deleted successfully.");
        }

        // GET /reports/orders
        [HttpGet]
        [Route("reports/orders")]
        public IHttpActionResult GetOrderReports()
        {
            var reports = new List<OrderReport>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM orders_reports", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    reports.Add(new OrderReport
                    {
                        Id = (int)reader["id"],
                        OrderId = (int)reader["order_id"],
                        TotalRevenue = (decimal)reader["total_revenue"],
                        TotalCost = (decimal)reader["total_cost"],
                        TotalProfit = (decimal)reader["total_profit"]
                    });
                }
            }
            return Ok(reports);
        }

        // GET /reports/orders/{id}
        [HttpGet]
        [Route("reports/orders/{id:int}")]
        public IHttpActionResult GetOrderReportById(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM orders_reports WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var report = new OrderReport
                    {
                        Id = (int)reader["id"],
                        OrderId = (int)reader["order_id"],
                        TotalRevenue = (decimal)reader["total_revenue"],
                        TotalCost = (decimal)reader["total_cost"],
                        TotalProfit = (decimal)reader["total_profit"]
                    };
                    return Ok(report);
                }
                return NotFound();
            }
        }

        // POST /reports/orders
        [HttpPost]
        [Route("reports/orders")]
        public IHttpActionResult CreateOrderReport(OrderReportRequest reportRequest)
        {
            try
            {
                // 1. Gọi API /orders để lấy thông tin đơn hàng
                var orders = GetOrders(Request);
                var order = orders.FirstOrDefault(o => o.Id == reportRequest.OrderId);
                if (order == null)
                {
                    return NotFound(); // Không tìm thấy đơn hàng
                }

                // 2. Gọi API /order_items để lấy thông tin chi tiết sản phẩm trong đơn hàng
                var orderItems = GetOrderItems(Request);
                var currentOrderItems = orderItems.Where(o => o.OrderId == reportRequest.OrderId);

                // 3. Tính toán TotalRevenue, TotalCost, và TotalProfit
                var totalRevenue = currentOrderItems.Sum(o => o.TotalPrice); // Tổng doanh thu
                var totalCost = reportRequest.TotalCost; // Chi phí từ client
                

                // 4. Lưu vào cơ sở dữ liệu
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "INSERT INTO orders_reports (order_id, total_revenue, total_cost) VALUES (@orderId, @revenue, @cost)",
                        connection
                    );
                    command.Parameters.AddWithValue("@orderId", reportRequest.OrderId);
                    command.Parameters.AddWithValue("@revenue", totalRevenue);
                    command.Parameters.AddWithValue("@cost", totalCost);
                   

                    command.ExecuteNonQuery();
                }

                return Ok("Order report created successfully.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        // DELETE /reports/orders/{id}
        [HttpDelete]
        [Route("reports/orders/{id:int}")]
        public IHttpActionResult DeleteOrderReport(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM orders_reports WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
            return Ok("Order report deleted successfully.");
        }
    }
}
