using HDV.Services;
using HDV_3.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;

namespace HDV_3.Controllers
{
    [JwtAuthentication]
    public class ProductsController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ProductDBConnection"].ConnectionString;

        // GET /products
        [HttpGet]
        [Route("products")]
        public IHttpActionResult GetProducts()
        {
            var products = new List<Product>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM products", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    products.Add(new Product
                    {
                        Id = (int)reader["id"],
                        Name = reader["name"].ToString(),
                        Description = reader["description"].ToString(),
                        Price = (decimal)reader["price"],
                        Quantity = (int)reader["quantity"],
                        CreatedAt = (DateTime)reader["created_at"],
                        UpdatedAt = (DateTime)reader["updated_at"]
                    });
                }
            }
            return Ok(products);
        }

        // GET /products/{id}
        [HttpGet]
        [Route("products/{id:int}")]
        public IHttpActionResult GetProduct(int id)
        {
            Product product = null;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM products WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    product = new Product
                    {
                        Id = (int)reader["id"],
                        Name = reader["name"].ToString(),
                        Description = reader["description"].ToString(),
                        Price = (decimal)reader["price"],
                        Quantity = (int)reader["quantity"],
                        CreatedAt = (DateTime)reader["created_at"],
                        UpdatedAt = (DateTime)reader["updated_at"]
                    };
                }
            }
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        // POST /products
        [HttpPost]
        [Route("products")]
        public IHttpActionResult AddProduct(Product product)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO products (name, description, price, quantity, created_at, updated_at) VALUES (@name, @description, @price, @quantity, GETDATE(), GETDATE())", connection);
                command.Parameters.AddWithValue("@name", product.Name);
                command.Parameters.AddWithValue("@description", product.Description);
                command.Parameters.AddWithValue("@price", product.Price);
                command.Parameters.AddWithValue("@quantity", product.Quantity);
                command.ExecuteNonQuery();
            }
            return Ok("Product added successfully.");
        }

        // PUT /products/{id}
        [HttpPut]
        [Route("products/{id:int}")]
        public IHttpActionResult UpdateProduct(int id, Product product)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE products SET name = @name, description = @description, price = @price, quantity = @quantity, updated_at = GETDATE() WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", product.Name);
                command.Parameters.AddWithValue("@description", product.Description);
                command.Parameters.AddWithValue("@price", product.Price);
                command.Parameters.AddWithValue("@quantity", product.Quantity);
                command.ExecuteNonQuery();
            }
            return Ok("Product updated successfully.");
        }

        // DELETE /products/{id}
        [HttpDelete]
        [Route("products/{id:int}")]
        public IHttpActionResult DeleteProduct(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM products WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
            return Ok("Product deleted successfully.");
        }
    }
}
