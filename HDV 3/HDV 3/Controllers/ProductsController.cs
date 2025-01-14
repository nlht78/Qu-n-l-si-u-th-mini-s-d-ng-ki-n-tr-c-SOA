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
        public IHttpActionResult GetProducts(int page = 1, int pageSize = 10)
        {
            var products = new List<Product>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var offset = (page - 1) * pageSize;
                var command = new SqlCommand(
                    "SELECT * FROM products ORDER BY id OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
                    connection);
                command.Parameters.AddWithValue("@offset", offset);
                command.Parameters.AddWithValue("@pageSize", pageSize);
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
                        CategoryId = (int)reader["categoryId"], // Bổ sung trường CategoryId
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
                        CategoryId = (int)reader["categoryId"], // Bổ sung trường CategoryId
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
                var command = new SqlCommand(
                    "INSERT INTO products (name, description, price, quantity, categoryId, created_at, updated_at) VALUES (@name, @description, @price, @quantity, @categoryId, GETDATE(), GETDATE())",
                    connection);
                command.Parameters.AddWithValue("@name", product.Name);
                command.Parameters.AddWithValue("@description", product.Description);
                command.Parameters.AddWithValue("@price", product.Price);
                command.Parameters.AddWithValue("@quantity", product.Quantity);
                command.Parameters.AddWithValue("@categoryId", product.CategoryId); // Thêm giá trị CategoryId
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
                var command = new SqlCommand(
                    "UPDATE products SET name = @name, description = @description, price = @price, quantity = @quantity, categoryId = @categoryId, updated_at = GETDATE() WHERE id = @id",
                    connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", product.Name);
                command.Parameters.AddWithValue("@description", product.Description);
                command.Parameters.AddWithValue("@price", product.Price);
                command.Parameters.AddWithValue("@quantity", product.Quantity);
                command.Parameters.AddWithValue("@categoryId", product.CategoryId); // Cập nhật giá trị CategoryId
                command.ExecuteNonQuery();
            }
            return Ok("Product updated successfully.");
        }

        // PUT /products/updatesl/{id}
        [HttpPut]
        [Route("products/updatesl/{id:int}")]
        public IHttpActionResult UpdateProductsl(int id, Product product)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "UPDATE products SET quantity = @quantity, updated_at = GETDATE() WHERE id = @id",
                    connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@quantity", product.Quantity);
                command.ExecuteNonQuery();
            }
            return Ok("Product updated quantity successfully.");
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

        // POST /categories
        [HttpPost]
        [Route("categories")]
        public IHttpActionResult AddCategory(Category category)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "INSERT INTO category (name, description, created_at, updated_at) VALUES (@name, @description, GETDATE(), GETDATE())",
                    connection);
                command.Parameters.AddWithValue("@name", category.Name);
                command.Parameters.AddWithValue("@description", category.Description);
                command.ExecuteNonQuery();
            }
            return Ok("Category added successfully.");
        }


        // GET /categories
        [HttpGet]
        [Route("categories")]
        public IHttpActionResult GetCategories()
        {
            var categories = new List<Category>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM category", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(new Category
                    {
                        Id = (int)reader["id"],
                        Name = reader["name"].ToString(),
                        Description = reader["description"].ToString(),
                        CreatedAt = (DateTime)reader["created_at"],
                        UpdatedAt = (DateTime)reader["updated_at"]
                    });
                }
            }
            return Ok(categories);
        }

        // PUT /categories/{id}
        [HttpPut]
        [Route("categories/{id:int}")]
        public IHttpActionResult UpdateCategory(int id, Category category)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "UPDATE category SET name = @name, description = @description, updated_at = GETDATE() WHERE id = @id",
                    connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", category.Name);
                command.Parameters.AddWithValue("@description", category.Description);
                command.ExecuteNonQuery();
            }
            return Ok("Category updated successfully.");
        }


        // DELETE /categories/{id}
        [HttpDelete]
        [Route("categories/{id:int}")]
        public IHttpActionResult DeleteCategory(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM category WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
            return Ok("Category deleted successfully.");
        }

        // GET /products/bycategory/{categoryId}
        [HttpGet]
        [Route("products/bycategory/{categoryId:int}")]
        public IHttpActionResult GetProductsByCategory(int categoryId, int page = 1, int pageSize = 10)
        {
            var products = new List<Product>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var offset = (page - 1) * pageSize;
                var command = new SqlCommand(
                    "SELECT * FROM products WHERE categoryId = @categoryId ORDER BY id OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY",
                    connection);
                command.Parameters.AddWithValue("@categoryId", categoryId);
                command.Parameters.AddWithValue("@offset", offset);
                command.Parameters.AddWithValue("@pageSize", pageSize);
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
                        CategoryId = (int)reader["categoryId"],
                        CreatedAt = (DateTime)reader["created_at"],
                        UpdatedAt = (DateTime)reader["updated_at"]
                    });
                }
            }
            return Ok(products);
        }
        // GET /categories/{id}
        [HttpGet]
        [Route("categories/{id:int}")]
        public IHttpActionResult GetCategoryById(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM category WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", id);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var category = new Category
                    {
                        Id = (int)reader["id"],
                        Name = reader["name"].ToString(),
                        Description = reader["description"].ToString(),
                        CreatedAt = (DateTime)reader["created_at"],
                        UpdatedAt = (DateTime)reader["updated_at"]
                    };
                    return Ok(category);
                }
                else
                {
                    return NotFound(); // Trả về 404 nếu không tìm thấy loại sản phẩm
                }
            }
        }





    }
}
