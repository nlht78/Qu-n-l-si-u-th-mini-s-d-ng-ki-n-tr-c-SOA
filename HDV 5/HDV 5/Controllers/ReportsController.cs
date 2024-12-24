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



        private Product GetProductDetails(int productId, HttpRequestMessage request)
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


                    var response = client.GetAsync($"products/{productId}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content.ReadAsAsync<Product>().Result;
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
        private bool CreateProductReportInternal(ProductReportRequest request, int orderReportId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "INSERT INTO product_reports (order_report_id, product_id, total_sold, revenue, cost) VALUES (@orderReportId, @productId, @totalSold, @revenue, @cost)",
                        connection
                    );
                    decimal Revenue = request.Revenue * request.TotalSold;
                    

                    command.Parameters.AddWithValue("@orderReportId", orderReportId);
                    command.Parameters.AddWithValue("@productId", request.ProductId);
                    command.Parameters.AddWithValue("@totalSold", request.TotalSold);
                    command.Parameters.AddWithValue("@revenue", Revenue);
                    command.Parameters.AddWithValue("@cost", request.Cost);
                    

                    command.ExecuteNonQuery();
                }

                return true; // Thành công
            }
            catch
            {
                return false; // Thất bại
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
        public IHttpActionResult CreateOrderReport(OrderReportRequest request)
        {
            try
            {
                decimal totalRevenue = 0;
                decimal totalCost = 0;

                List<ProductReportRequest> processedProducts = new List<ProductReportRequest>();

                foreach (var product in request.Products)
                {
                    var productDetails = GetProductDetails(product.ProductId, Request);
                    if (productDetails == null)
                    {
                        return NotFound();
                    }

                    decimal cost = (product.Revenue - (product.Revenue * 0.25m))* product.TotalSold;
                    

                    totalRevenue += product.Revenue * product.TotalSold;
                    totalCost += cost;

                    processedProducts.Add(new ProductReportRequest
                    {
                        ProductId = product.ProductId,
                        TotalSold = product.TotalSold,
                        Revenue = product.Revenue,
                        Cost = cost
                    });
                }

                int orderReportId;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "INSERT INTO orders_reports (order_id, total_revenue, total_cost) OUTPUT INSERTED.id VALUES (@orderId, @totalRevenue, @totalCost)",
                        connection
                    );
                    command.Parameters.AddWithValue("@orderId", request.OrderId);
                    command.Parameters.AddWithValue("@totalRevenue", totalRevenue);
                    command.Parameters.AddWithValue("@totalCost", totalCost);
                    

                    orderReportId = (int)command.ExecuteScalar();
                }

                foreach (var product in processedProducts)
                {
                    var productResponse = CreateProductReportInternal(product, orderReportId);
                    if (!productResponse)
                    {
                        return InternalServerError(new Exception($"Failed to create product report for ProductId: {product.ProductId}"));
                    }
                }

                return Ok("Order report and associated product reports created successfully.");
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
