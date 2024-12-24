using HDV_4.Models;
using HDV_4.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Web.Http;

namespace HDV_4.Controllers
{
    // Xác thực qua JWT
    [JwtAuthentication]
    public class OrdersController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["OrderDBConnection"].ConnectionString;

        // GET /orders
        [HttpGet]
        [Route("orders")]
        public IHttpActionResult GetOrders()
        {
            var orders = new List<Order>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM orders", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    orders.Add(new Order
                    {
                        Id = (int)reader["id"],
                        CustomerName = reader["customer_name"].ToString(),
                        CustomerEmail = reader["customer_email"].ToString(),
                        TotalAmount = (decimal)reader["total_amount"],
                        Status = reader["status"].ToString(),
                        CreatedAt = (DateTime)reader["created_at"],
                        UpdatedAt = (DateTime)reader["updated_at"]
                    });
                }
            }
            return Ok(orders);
        }


        // GET /orders/pending
        [HttpGet]
        [Route("orders/pending")]
        public IHttpActionResult GetPendingOrders()
        {
            var pendingOrders = new List<Order>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM orders WHERE status = @status", connection);
                command.Parameters.AddWithValue("@status", "pending");
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    pendingOrders.Add(new Order
                    {
                        Id = (int)reader["id"],
                        CustomerName = reader["customer_name"].ToString(),
                        CustomerEmail = reader["customer_email"].ToString(),
                        TotalAmount = (decimal)reader["total_amount"],
                        Status = reader["status"].ToString(),
                        CreatedAt = (DateTime)reader["created_at"],
                        UpdatedAt = (DateTime)reader["updated_at"]
                    });
                }
            }
            return Ok(pendingOrders);
        }

        // GET /orders/{id}
        [HttpGet]
        [Route("orders/{id:int}")]
        public IHttpActionResult GetOrderById(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM orders WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var order = new Order
                    {
                        Id = (int)reader["id"],
                        CustomerName = reader["customer_name"].ToString(),
                        CustomerEmail = reader["customer_email"].ToString(),
                        TotalAmount = (decimal)reader["total_amount"],
                        Status = reader["status"].ToString(),
                        CreatedAt = (DateTime)reader["created_at"],
                        UpdatedAt = (DateTime)reader["updated_at"]
                    };
                    return Ok(order);
                }
                return NotFound();
            }
        }


        // POST /orders
        [HttpPost]
        [Route("orders")]
        public IHttpActionResult CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null || request.Order == null)
            {
                return BadRequest("Order data is required.");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // 1. Thêm đơn hàng vào bảng orders
                    var command = new SqlCommand(
                        "INSERT INTO orders (customer_name, customer_email, total_amount, status, created_at, updated_at) " +
                        "VALUES (@name, @email, @amount, @status, GETDATE(), GETDATE()); SELECT SCOPE_IDENTITY();",
                        connection, transaction
                    );
                    command.Parameters.AddWithValue("@name", request.Order.CustomerName);
                    command.Parameters.AddWithValue("@email", request.Order.CustomerEmail);
                    command.Parameters.AddWithValue("@amount", 0); // TotalAmount mặc định là 0
                    command.Parameters.AddWithValue("@status", "pending"); // Trạng thái mặc định là "pending"

                    var orderId = Convert.ToInt32(command.ExecuteScalar());

                    // 2. Kiểm tra nếu có orderItems
                    if (request.OrderItems != null && request.OrderItems.Count > 0)
                    {
                        foreach (var item in request.OrderItems)
                        {
                            // 2.1 Thêm các mục vào bảng order_items
                            var itemCommand = new SqlCommand(
                                "INSERT INTO order_items (order_id, product_id, product_name, quantity, unit_price) " +
                                "VALUES (@orderId, @productId, @name, @quantity, @price)",
                                connection, transaction
                            );
                            itemCommand.Parameters.AddWithValue("@orderId", orderId);
                            itemCommand.Parameters.AddWithValue("@productId", item.ProductId);
                            itemCommand.Parameters.AddWithValue("@name", item.ProductName);
                            itemCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                            itemCommand.Parameters.AddWithValue("@price", item.UnitPrice);
                            
                            itemCommand.ExecuteNonQuery();

                            // 2.2 Lấy số lượng hiện tại của sản phẩm
                            int currentQuantity;
                            using (var getProductClient = new HttpClient())
                            {
                                getProductClient.BaseAddress = new Uri("https://localhost:44361/");

                                // Lấy Authorization Header từ request gốc
                                if (Request.Headers.Authorization == null || string.IsNullOrEmpty(Request.Headers.Authorization.Parameter))
                                {
                                    throw new Exception("Authorization header is missing or invalid.");
                                }

                                var jwtToken = Request.Headers.Authorization.Parameter;

                                // Thêm Authorization Key vào Header
                                getProductClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                                // Gửi yêu cầu GET để lấy thông tin sản phẩm
                                var productResponse = getProductClient.GetAsync($"products/{item.ProductId}").Result;

                                if (!productResponse.IsSuccessStatusCode)
                                {
                                    throw new Exception($"Failed to fetch product details for ProductId: {item.ProductId}. Status: {productResponse.StatusCode}");
                                }

                                var product = productResponse.Content.ReadAsAsync<Product>().Result;
                                currentQuantity = product.Quantity;
                            }

                            // 2.3 Tính toán số lượng mới
                            int updatedQuantity = currentQuantity - item.Quantity;

                            // 2.4 Gửi yêu cầu PUT để cập nhật số lượng sản phẩm
                            using (var updateClient = new HttpClient())
                            {
                                updateClient.BaseAddress = new Uri("https://localhost:44361/");

                                // Thêm Authorization Key vào Header
                                updateClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Request.Headers.Authorization.Parameter);

                                var updateResponse = updateClient.PutAsJsonAsync($"products/updatesl/{item.ProductId}", new
                                {
                                    Quantity = updatedQuantity
                                }).Result;

                                if (!updateResponse.IsSuccessStatusCode)
                                {
                                    var error = updateResponse.Content.ReadAsStringAsync().Result;
                                    throw new Exception($"Failed to update product quantity for ProductId: {item.ProductId}. Status: {updateResponse.StatusCode}, Error: {error}");
                                }
                            }
                        }

                        // 3. Cập nhật tổng tiền của đơn hàng (total_amount)
                        var updateOrderCommand = new SqlCommand(
                            "UPDATE orders SET total_amount = (SELECT SUM(total_price) FROM order_items WHERE order_id = @orderId) " +
                            "WHERE id = @orderId",
                            connection, transaction
                        );
                        updateOrderCommand.Parameters.AddWithValue("@orderId", orderId);
                        updateOrderCommand.ExecuteNonQuery();
                    }

                    // Commit transaction
                    transaction.Commit();

                    return Ok(new
                    {
                        OrderId = orderId,
                        Message = request.OrderItems != null && request.OrderItems.Count > 0
                            ? "Order and order items created successfully."
                            : "Order created successfully without order items."
                    });
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }
        }





        // PUT /orders/{id}
        [HttpPut]
        [Route("orders/{id:int}")]
        public IHttpActionResult UpdateOrderStatus(int id, Order order)

        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE orders SET status = @status, updated_at = GETDATE() WHERE id = @id", connection);
                command.Parameters.AddWithValue("@status", order.Status);
                command.Parameters.AddWithValue("@id", id);
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok("Order status updated successfully.");
                }
                return NotFound();
            }
        }


        
        // DELETE /orders/{id}
        [HttpDelete]
        [Route("orders/{id:int}")]
        public IHttpActionResult DeleteOrder(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction(); // Bắt đầu transaction

                try
                {
                    // 1. Lấy các chi tiết đơn hàng (order_items) của đơn hàng
                    var orderItems = new List<OrderItem>();
                    var getOrderItemsCommand = new SqlCommand("SELECT * FROM order_items WHERE order_id = @orderId", connection, transaction);
                    getOrderItemsCommand.Parameters.AddWithValue("@orderId", id);

                    using (var reader = getOrderItemsCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orderItems.Add(new OrderItem
                            {
                                ProductId = (int)reader["product_id"],
                                Quantity = (int)reader["quantity"]
                            });
                        }
                    }

                    // 2. Cập nhật số lượng sản phẩm thông qua API
                    foreach (var item in orderItems)
                    {
                        // 2.1 Lấy số lượng hiện tại của sản phẩm
                        int currentQuantity;
                        using (var getProductClient = new HttpClient())
                        {
                            getProductClient.BaseAddress = new Uri("https://localhost:44361/");

                            // Lấy Authorization Header từ request gốc
                            if (Request.Headers.Authorization == null || string.IsNullOrEmpty(Request.Headers.Authorization.Parameter))
                            {
                                throw new Exception("Authorization header is missing or invalid.");
                            }

                            var jwtToken = Request.Headers.Authorization.Parameter;

                            // Thêm Authorization Key vào Header
                            getProductClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                            // Gửi yêu cầu GET để lấy thông tin sản phẩm
                            var productResponse = getProductClient.GetAsync($"products/{item.ProductId}").Result;

                            if (!productResponse.IsSuccessStatusCode)
                            {
                                throw new Exception($"Failed to fetch product details for ProductId: {item.ProductId}. Status: {productResponse.StatusCode}");
                            }

                            var product = productResponse.Content.ReadAsAsync<Product>().Result;
                            currentQuantity = product.Quantity;
                        }

                        // 2.2 Tính toán số lượng mới
                        int updatedQuantity = currentQuantity + item.Quantity;

                        // 2.3 Gửi yêu cầu cập nhật số lượng sản phẩm
                        using (var updateClient = new HttpClient())
                        {
                            updateClient.BaseAddress = new Uri("https://localhost:44361/");

                            // Thêm Authorization Key vào Header
                            updateClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Request.Headers.Authorization.Parameter);

                            // Gửi yêu cầu PUT để cập nhật số lượng sản phẩm
                            var updateResponse = updateClient.PutAsJsonAsync($"products/updatesl/{item.ProductId}", new
                            {
                                Quantity = updatedQuantity
                            }).Result;

                            if (!updateResponse.IsSuccessStatusCode)
                            {
                                var error = updateResponse.Content.ReadAsStringAsync().Result;
                                throw new Exception($"Failed to update product quantity for ProductId: {item.ProductId}. Status: {updateResponse.StatusCode}, Error: {error}");
                            }
                        }
                    }

                    // 3. Xóa chi tiết đơn hàng khỏi bảng order_items
                    var deleteOrderItemsCommand = new SqlCommand("DELETE FROM order_items WHERE order_id = @orderId", connection, transaction);
                    deleteOrderItemsCommand.Parameters.AddWithValue("@orderId", id);
                    deleteOrderItemsCommand.ExecuteNonQuery();

                    // 4. Xóa đơn hàng khỏi bảng orders
                    var deleteOrderCommand = new SqlCommand("DELETE FROM orders WHERE id = @id", connection, transaction);
                    deleteOrderCommand.Parameters.AddWithValue("@id", id);
                    var rowsAffected = deleteOrderCommand.ExecuteNonQuery();

                    if (rowsAffected <= 0)
                    {
                        throw new Exception("Order not found or failed to delete.");
                    }

                    transaction.Commit(); // Commit transaction nếu tất cả các bước đều thành công
                    return Ok("Order and associated items deleted successfully, and product quantities updated.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback transaction nếu xảy ra lỗi
                    return InternalServerError(ex);
                }
            }
        }






    }
}
