using HDV_4.Models;
using HDV_4.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        public IHttpActionResult CreateOrder(Order order, List<OrderItem> items)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    // Tạo đơn hàng
                    var command = new SqlCommand("INSERT INTO orders (customer_name, customer_email, total_amount, status) VALUES (@name, @email, @amount, @status); SELECT SCOPE_IDENTITY();", connection, transaction);
                    command.Parameters.AddWithValue("@name", order.CustomerName);
                    command.Parameters.AddWithValue("@email", order.CustomerEmail);
                    command.Parameters.AddWithValue("@amount", order.TotalAmount);
                    command.Parameters.AddWithValue("@status", order.Status);
                    var orderId = Convert.ToInt32(command.ExecuteScalar());

                    // Thêm chi tiết đơn hàng
                    foreach (var item in items)
                    {
                        var itemCommand = new SqlCommand("INSERT INTO order_items (order_id, product_id, product_name, quantity, unit_price) VALUES (@orderId, @productId, @name, @quantity, @price)", connection, transaction);
                        itemCommand.Parameters.AddWithValue("@orderId", orderId);
                        itemCommand.Parameters.AddWithValue("@productId", item.ProductId);
                        itemCommand.Parameters.AddWithValue("@name", item.ProductName);
                        itemCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                        itemCommand.Parameters.AddWithValue("@price", item.UnitPrice);
                        itemCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return Ok("Order created successfully.");
                }
                catch
                {
                    transaction.Rollback();
                    return InternalServerError();
                }
            }
        }

        // PUT /orders/{id}
        [HttpPut]
        [Route("orders/{id:int}")]
        public IHttpActionResult UpdateOrderStatus(int id, string status)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE orders SET status = @status, updated_at = GETDATE() WHERE id = @id", connection);
                command.Parameters.AddWithValue("@status", status);
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
                var command = new SqlCommand("DELETE FROM orders WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok("Order deleted successfully.");
                }
                return NotFound();
            }
        }


            
        }
}
