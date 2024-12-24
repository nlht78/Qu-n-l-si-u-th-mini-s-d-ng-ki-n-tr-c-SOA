using HDV_4.Models;
using HDV_4.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Web.Http;

namespace HDV_4.Controllers
{
    [JwtAuthentication] // Xác thực qua JWT
    public class OrderItemsController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["OrderDBConnection"].ConnectionString;

        // GET /order_items
        [HttpGet]
        [Route("order_items")]
        public IHttpActionResult GetAllOrderItems()
        {
            var orderItems = new List<OrderItem>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM order_items", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    orderItems.Add(new OrderItem
                    {
                        Id = (int)reader["id"],
                        OrderId = (int)reader["order_id"],
                        ProductId = (int)reader["product_id"],
                        ProductName = reader["product_name"].ToString(),
                        Quantity = (int)reader["quantity"],
                        UnitPrice = (decimal)reader["unit_price"],
                        TotalPrice = (decimal)reader["total_price"]
                    });
                }
            }
            return Ok(orderItems);
        }

        // GET /order_items/{id}
        [HttpGet]
        [Route("order_items/{id:int}")]
        public IHttpActionResult GetOrderItemById(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM order_items WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var orderItem = new OrderItem
                    {
                        Id = (int)reader["id"],
                        OrderId = (int)reader["order_id"],
                        ProductId = (int)reader["product_id"],
                        ProductName = reader["product_name"].ToString(),
                        Quantity = (int)reader["quantity"],
                        UnitPrice = (decimal)reader["unit_price"],
                        TotalPrice = (decimal)reader["total_price"]
                    };
                    return Ok(orderItem);
                }
                return NotFound();
            }
        }

        [HttpGet]
        [Route("order_items")]
        public IHttpActionResult GetOrderItemsByOrderId(int orderId)
        {
            var orderItems = new List<OrderItem>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM order_items WHERE order_id = @orderId", connection);
                command.Parameters.AddWithValue("@orderId", orderId);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    orderItems.Add(new OrderItem
                    {
                        Id = (int)reader["id"],
                        OrderId = (int)reader["order_id"],
                        ProductId = (int)reader["product_id"],
                        ProductName = reader["product_name"].ToString(),
                        Quantity = (int)reader["quantity"],
                        UnitPrice = (decimal)reader["unit_price"],
                        TotalPrice = (decimal)reader["total_price"]
                    });
                }
            }
            return Ok(orderItems);
        }



        // POST /order_items
        [HttpPost]
        [Route("order_items")]
        public IHttpActionResult CreateOrderItem(OrderItem item)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // 1. Thêm sản phẩm vào bảng order_items
                    var command = new SqlCommand(
                        "INSERT INTO order_items (order_id, product_id, product_name, quantity, unit_price) " +
                        "VALUES (@orderId, @productId, @name, @quantity, @price)", connection, transaction);
                    command.Parameters.AddWithValue("@orderId", item.OrderId);
                    command.Parameters.AddWithValue("@productId", item.ProductId);
                    command.Parameters.AddWithValue("@name", item.ProductName);
                    command.Parameters.AddWithValue("@quantity", item.Quantity);
                    command.Parameters.AddWithValue("@price", item.UnitPrice);
                    

                    var rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected <= 0)
                    {
                        transaction.Rollback();
                        return BadRequest("Failed to create order item.");
                    }

                    // 2. Cập nhật tổng tiền của đơn hàng
                    var updateOrderCommand = new SqlCommand(
                        "UPDATE orders SET total_amount = (SELECT SUM(total_price) FROM order_items WHERE order_id = @orderId) " +
                        "WHERE id = @orderId", connection, transaction);
                    updateOrderCommand.Parameters.AddWithValue("@orderId", item.OrderId);
                    updateOrderCommand.ExecuteNonQuery();

                    // 3. Lấy số lượng hiện tại của sản phẩm từ API
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
                        getProductClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                        var productResponse = getProductClient.GetAsync($"products/{item.ProductId}").Result;
                        if (!productResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to fetch product details for ProductId: {item.ProductId}. Status: {productResponse.StatusCode}");
                        }

                        var product = productResponse.Content.ReadAsAsync<Product>().Result;
                        currentQuantity = product.Quantity;
                    }

                    // 4. Tính toán số lượng mới và cập nhật sản phẩm
                    int updatedQuantity = currentQuantity - item.Quantity;
                    using (var updateClient = new HttpClient())
                    {
                        updateClient.BaseAddress = new Uri("https://localhost:44361/");
                        updateClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Request.Headers.Authorization.Parameter);

                        var updateResponse = updateClient.PutAsJsonAsync($"products/updatesl/{item.ProductId}", new { Quantity = updatedQuantity }).Result;

                        if (!updateResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to update product quantity for ProductId: {item.ProductId}. Status: {updateResponse.StatusCode}");
                        }
                    }

                    // Commit transaction
                    transaction.Commit();

                    return Ok("Order item created successfully and total amount updated.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }
        }

        // PUT /order_items/{orderId}/product/{productId}/quantity
        [HttpPut]
        [Route("order_items/{orderId:int}/product/{productId:int}/quantity")]
        public IHttpActionResult UpdateOrderItemQuantity(int orderId, int productId, [FromBody] OrderItem updatedDetails)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // Lấy thông tin chi tiết sản phẩm từ bảng order_items
                    OrderItem orderItem = null;
                    var getOrderItemCommand = new SqlCommand(
                        "SELECT * FROM order_items WHERE order_id = @orderId AND product_id = @productId",
                        connection, transaction);
                    getOrderItemCommand.Parameters.AddWithValue("@orderId", orderId);
                    getOrderItemCommand.Parameters.AddWithValue("@productId", productId);

                    using (var reader = getOrderItemCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            orderItem = new OrderItem
                            {
                                Id = (int)reader["id"],
                                OrderId = (int)reader["order_id"],
                                ProductId = (int)reader["product_id"],
                                ProductName = reader["product_name"].ToString(),
                                Quantity = (int)reader["quantity"],
                                UnitPrice = (decimal)reader["unit_price"],
                                TotalPrice = (decimal)reader["total_price"]
                            };
                        }
                    }

                    if (orderItem == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật số lượng sản phẩm trong bảng order_items
                    var updateQuantityCommand = new SqlCommand(
                        "UPDATE order_items SET quantity = @quantity " +
                        "WHERE order_id = @orderId AND product_id = @productId",
                        connection, transaction);
                    updateQuantityCommand.Parameters.AddWithValue("@quantity", updatedDetails.Quantity);
                    
                    updateQuantityCommand.Parameters.AddWithValue("@orderId", orderId);
                    updateQuantityCommand.Parameters.AddWithValue("@productId", productId);
                    updateQuantityCommand.ExecuteNonQuery();

                    // Cập nhật tổng tiền của đơn hàng
                    var updateOrderCommand = new SqlCommand(
                        "UPDATE orders SET total_amount = (SELECT SUM(total_price) FROM order_items WHERE order_id = @orderId) WHERE id = @orderId",
                        connection, transaction);
                    updateOrderCommand.Parameters.AddWithValue("@orderId", orderId);
                    updateOrderCommand.ExecuteNonQuery();

                    // Cập nhật số lượng sản phẩm trong kho
                    int currentQuantity;
                    using (var getProductClient = new HttpClient())
                    {
                        getProductClient.BaseAddress = new Uri("https://localhost:44361/");
                        getProductClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Request.Headers.Authorization.Parameter);

                        var productResponse = getProductClient.GetAsync($"products/{productId}").Result;
                        if (!productResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to fetch product details for ProductId: {productId}. Status: {productResponse.StatusCode}");
                        }

                        var product = productResponse.Content.ReadAsAsync<Product>().Result;
                        currentQuantity = product.Quantity;
                    }

                    // Tính toán số lượng cập nhật
                    int updatedQuantity = currentQuantity - (updatedDetails.Quantity - orderItem.Quantity);

                    using (var updateProductClient = new HttpClient())
                    {
                        updateProductClient.BaseAddress = new Uri("https://localhost:44361/");
                        updateProductClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Request.Headers.Authorization.Parameter);

                        var updateProductResponse = updateProductClient.PutAsJsonAsync($"products/updatesl/{productId}", new { Quantity = updatedQuantity }).Result;
                        if (!updateProductResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to update product quantity for ProductId: {productId}. Status: {updateProductResponse.StatusCode}");
                        }
                    }

                    // Commit transaction
                    transaction.Commit();

                    return Ok("Order item quantity updated successfully.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }
        }


        // PUT /order_items/{orderId}/product/{productId}/details
        [HttpPut]
        [Route("order_items/{orderId:int}/product/{productId:int}/details")]
        public IHttpActionResult UpdateOrderItemDetails(int orderId, int productId, [FromBody] OrderItem updatedDetails)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // Lấy thông tin sản phẩm từ bảng order_items
                    OrderItem orderItem = null;
                    var getOrderItemCommand = new SqlCommand(
                        "SELECT * FROM order_items WHERE order_id = @orderId AND product_id = @productId",
                        connection, transaction);
                    getOrderItemCommand.Parameters.AddWithValue("@orderId", orderId);
                    getOrderItemCommand.Parameters.AddWithValue("@productId", productId);

                    using (var reader = getOrderItemCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            orderItem = new OrderItem
                            {
                                Id = (int)reader["id"],
                                OrderId = (int)reader["order_id"],
                                ProductId = (int)reader["product_id"],
                                ProductName = reader["product_name"].ToString(),
                                Quantity = (int)reader["quantity"],
                                UnitPrice = (decimal)reader["unit_price"],
                                TotalPrice = (decimal)reader["total_price"]
                            };
                        }
                    }

                    if (orderItem == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin sản phẩm trong bảng order_items
                    var updateDetailsCommand = new SqlCommand(
                        "UPDATE order_items SET quantity = @quantity, unit_price = @unitPrice " +
                        "WHERE order_id = @orderId AND product_id = @productId",
                        connection, transaction);
                    updateDetailsCommand.Parameters.AddWithValue("@quantity", updatedDetails.Quantity);
                    updateDetailsCommand.Parameters.AddWithValue("@unitPrice", updatedDetails.UnitPrice);
                    
                    updateDetailsCommand.Parameters.AddWithValue("@orderId", orderId);
                    updateDetailsCommand.Parameters.AddWithValue("@productId", productId);
                    updateDetailsCommand.ExecuteNonQuery();

                    // Cập nhật tổng tiền của đơn hàng
                    var updateOrderCommand = new SqlCommand(
                        "UPDATE orders SET total_amount = (SELECT SUM(total_price) FROM order_items WHERE order_id = @orderId) WHERE id = @orderId",
                        connection, transaction);
                    updateOrderCommand.Parameters.AddWithValue("@orderId", orderId);
                    updateOrderCommand.ExecuteNonQuery();

                    // Cập nhật số lượng sản phẩm trong kho
                    int currentQuantity;
                    using (var getProductClient = new HttpClient())
                    {
                        getProductClient.BaseAddress = new Uri("https://localhost:44361/");
                        getProductClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Request.Headers.Authorization.Parameter);

                        var productResponse = getProductClient.GetAsync($"products/{productId}").Result;
                        if (!productResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to fetch product details for ProductId: {productId}. Status: {productResponse.StatusCode}");
                        }

                        var product = productResponse.Content.ReadAsAsync<Product>().Result;
                        currentQuantity = product.Quantity;
                    }

                    // Tính toán số lượng cập nhật
                    int updatedQuantity = currentQuantity - (updatedDetails.Quantity - orderItem.Quantity);

                    using (var updateProductClient = new HttpClient())
                    {
                        updateProductClient.BaseAddress = new Uri("https://localhost:44361/");
                        updateProductClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Request.Headers.Authorization.Parameter);

                        var updateProductResponse = updateProductClient.PutAsJsonAsync($"products/updatesl/{productId}", new { Quantity = updatedQuantity }).Result;
                        if (!updateProductResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to update product quantity for ProductId: {productId}. Status: {updateProductResponse.StatusCode}");
                        }
                    }

                    // Commit transaction
                    transaction.Commit();

                    return Ok("Order item details updated successfully.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }
        }



        // PUT /order_items/{id}
        [HttpPut]
        [Route("order_items/{id:int}")]
        public IHttpActionResult UpdateOrderItem(int id, OrderItem item)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE order_items SET product_id = @productId, product_name = @name, quantity = @quantity, unit_price = @price WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@productId", item.ProductId);
                command.Parameters.AddWithValue("@name", item.ProductName);
                command.Parameters.AddWithValue("@quantity", item.Quantity);
                command.Parameters.AddWithValue("@price", item.UnitPrice);

                var rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return Ok("Order item updated successfully.");
                }
                return NotFound();
            }
        }

        // DELETE /order_items/{id}
        [HttpDelete]
        [Route("order_items/{id:int}")]
        public IHttpActionResult DeleteOrderItem(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // 1. Lấy chi tiết sản phẩm từ bảng order_items
                    OrderItem orderItem = null;
                    var getOrderItemCommand = new SqlCommand("SELECT * FROM order_items WHERE id = @id", connection, transaction);
                    getOrderItemCommand.Parameters.AddWithValue("@id", id);
                    using (var reader = getOrderItemCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            orderItem = new OrderItem
                            {
                                Id = (int)reader["id"],
                                OrderId = (int)reader["order_id"],
                                ProductId = (int)reader["product_id"],
                                ProductName = reader["product_name"].ToString(),
                                Quantity = (int)reader["quantity"],
                                UnitPrice = (decimal)reader["unit_price"],
                                TotalPrice = (decimal)reader["total_price"]
                            };
                        }
                    }

                    if (orderItem == null)
                    {
                        return NotFound();
                    }

                    // 2. Xóa sản phẩm khỏi bảng order_items
                    var deleteOrderItemCommand = new SqlCommand("DELETE FROM order_items WHERE id = @id", connection, transaction);
                    deleteOrderItemCommand.Parameters.AddWithValue("@id", id);
                    deleteOrderItemCommand.ExecuteNonQuery();

                    // 3. Cập nhật tổng tiền của đơn hàng
                    var updateOrderCommand = new SqlCommand(
                        "UPDATE orders SET total_amount = (SELECT SUM(total_price) FROM order_items WHERE order_id = @orderId) " +
                        "WHERE id = @orderId", connection, transaction);
                    updateOrderCommand.Parameters.AddWithValue("@orderId", orderItem.OrderId);
                    updateOrderCommand.ExecuteNonQuery();

                    // 4. Lấy số lượng hiện tại của sản phẩm từ API
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
                        getProductClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                        var productResponse = getProductClient.GetAsync($"products/{orderItem.ProductId}").Result;
                        if (!productResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to fetch product details for ProductId: {orderItem.ProductId}. Status: {productResponse.StatusCode}");
                        }

                        var product = productResponse.Content.ReadAsAsync<Product>().Result;
                        currentQuantity = product.Quantity;
                    }

                    // 5. Tính toán số lượng mới và cập nhật sản phẩm
                    int updatedQuantity = currentQuantity + orderItem.Quantity;
                    using (var updateClient = new HttpClient())
                    {
                        updateClient.BaseAddress = new Uri("https://localhost:44361/");
                        updateClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Request.Headers.Authorization.Parameter);

                        var updateResponse = updateClient.PutAsJsonAsync($"products/updatesl/{orderItem.ProductId}", new { Quantity = updatedQuantity }).Result;

                        if (!updateResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to update product quantity for ProductId: {orderItem.ProductId}. Status: {updateResponse.StatusCode}");
                        }
                    }

                    // Commit transaction
                    transaction.Commit();

                    return Ok("Order item deleted successfully and product quantity updated.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }
        }
        // DELETE /order_items/order/{orderId}/product/{productId}
        [HttpDelete]
        [Route("order_items/order/{orderId:int}/product/{productId:int}")]
        public IHttpActionResult DeleteProductFromOrder(int orderId, int productId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // 1. Lấy chi tiết sản phẩm từ bảng order_items
                    OrderItem orderItem = null;
                    var getOrderItemCommand = new SqlCommand(
                        "SELECT * FROM order_items WHERE order_id = @orderId AND product_id = @productId", connection, transaction);
                    getOrderItemCommand.Parameters.AddWithValue("@orderId", orderId);
                    getOrderItemCommand.Parameters.AddWithValue("@productId", productId);

                    using (var reader = getOrderItemCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            orderItem = new OrderItem
                            {
                                Id = (int)reader["id"],
                                OrderId = (int)reader["order_id"],
                                ProductId = (int)reader["product_id"],
                                ProductName = reader["product_name"].ToString(),
                                Quantity = (int)reader["quantity"],
                                UnitPrice = (decimal)reader["unit_price"],
                                TotalPrice = (decimal)reader["total_price"]
                            };
                        }
                    }

                    if (orderItem == null)
                    {
                        return NotFound();
                    }

                    // 2. Xóa sản phẩm khỏi bảng order_items
                    var deleteOrderItemCommand = new SqlCommand(
                        "DELETE FROM order_items WHERE order_id = @orderId AND product_id = @productId", connection, transaction);
                    deleteOrderItemCommand.Parameters.AddWithValue("@orderId", orderId);
                    deleteOrderItemCommand.Parameters.AddWithValue("@productId", productId);
                    deleteOrderItemCommand.ExecuteNonQuery();

                    // 3. Cập nhật tổng tiền của đơn hàng
                    var updateOrderCommand = new SqlCommand(
                        "UPDATE orders SET total_amount = (SELECT SUM(total_price) FROM order_items WHERE order_id = @orderId) " +
                        "WHERE id = @orderId", connection, transaction);
                    updateOrderCommand.Parameters.AddWithValue("@orderId", orderId);
                    updateOrderCommand.ExecuteNonQuery();

                    // 4. Lấy số lượng hiện tại của sản phẩm từ API
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
                        getProductClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                        var productResponse = getProductClient.GetAsync($"products/{productId}").Result;
                        if (!productResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to fetch product details for ProductId: {productId}. Status: {productResponse.StatusCode}");
                        }

                        var product = productResponse.Content.ReadAsAsync<Product>().Result;
                        currentQuantity = product.Quantity;
                    }

                    // 5. Tính toán số lượng mới và cập nhật sản phẩm
                    int updatedQuantity = currentQuantity + orderItem.Quantity;
                    using (var updateClient = new HttpClient())
                    {
                        updateClient.BaseAddress = new Uri("https://localhost:44361/");
                        updateClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Request.Headers.Authorization.Parameter);

                        var updateResponse = updateClient.PutAsJsonAsync($"products/updatesl/{productId}", new { Quantity = updatedQuantity }).Result;

                        if (!updateResponse.IsSuccessStatusCode)
                        {
                            throw new Exception($"Failed to update product quantity for ProductId: {productId}. Status: {updateResponse.StatusCode}");
                        }
                    }

                    // Commit transaction
                    transaction.Commit();

                    return Ok($"Product (ID: {productId}) removed from Order (ID: {orderId}), and product quantity updated.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }
        }


    }
}
