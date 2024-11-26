using HDV_4.Models;
using HDV_4.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        // POST /order_items
        [HttpPost]
        [Route("order_items")]
        public IHttpActionResult CreateOrderItem(OrderItem item)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO order_items (order_id, product_id, product_name, quantity, unit_price) VALUES (@orderId, @productId, @name, @quantity, @price)", connection);
                command.Parameters.AddWithValue("@orderId", item.OrderId);
                command.Parameters.AddWithValue("@productId", item.ProductId);
                command.Parameters.AddWithValue("@name", item.ProductName);
                command.Parameters.AddWithValue("@quantity", item.Quantity);
                command.Parameters.AddWithValue("@price", item.UnitPrice);

                var rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return Ok("Order item created successfully.");
                }
                return BadRequest("Failed to create order item.");
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
                var command = new SqlCommand("DELETE FROM order_items WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);

                var rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return Ok("Order item deleted successfully.");
                }
                return NotFound();
            }
        }
    }
}
