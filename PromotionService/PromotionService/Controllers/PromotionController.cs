using PromotionService.Models;
using PromotionService.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Web.Http;

namespace PromotionService.Controllers
{
    [JwtAuthentication] // Yêu cầu xác thực JWT
    public class PromotionController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PromotionDBConnection"].ConnectionString;



        // Lấy danh sách tất cả chương trình khuyến mãi
        [HttpGet]
        [Route("promotions")]
        public IHttpActionResult GetPromotions()
        {
            var promotions = new List<Promotion>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM promotions", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    promotions.Add(new Promotion
                    {
                        Id = (int)reader["id"],
                        Name = reader["name"].ToString(),
                        Description = reader["description"].ToString(),
                        DiscountPercent = (decimal)reader["discount_percent"],
                        StartDate = (DateTime)reader["start_date"],
                        EndDate = (DateTime)reader["end_date"],
                        CreatedAt = (DateTime)reader["created_at"],
                        UpdatedAt = (DateTime)reader["updated_at"]
                    });
                }
            }
            return Ok(promotions);
        }

        // Thêm chương trình khuyến mãi mới
        [HttpPost]
        [Route("promotions")]
        public IHttpActionResult AddPromotion(Promotion promotion)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "INSERT INTO promotions (name, description, discount_percent, start_date, end_date, created_at, updated_at) " +
                    "VALUES (@name, @description, @discount_percent, @start_date, @end_date, GETDATE(), GETDATE())",
                    connection);
                command.Parameters.AddWithValue("@name", promotion.Name);
                command.Parameters.AddWithValue("@description", promotion.Description);
                command.Parameters.AddWithValue("@discount_percent", promotion.DiscountPercent);
                command.Parameters.AddWithValue("@start_date", promotion.StartDate);
                command.Parameters.AddWithValue("@end_date", promotion.EndDate);
                command.ExecuteNonQuery();
            }
            return Ok("Promotion added successfully.");
        }

        // Thêm sản phẩm vào chương trình khuyến mãi
        [HttpPost]
        [Route("promotions/{promotionId}/products")]
        public IHttpActionResult AddProductToPromotion(int promotionId, int productId, HttpRequestMessage request)
        {
            // Gọi dịch vụ quản lý sản phẩm để xác thực sản phẩm tồn tại
            if (!ValidateProduct(productId, request))
            {
                return BadRequest("Product does not exist.");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "INSERT INTO promotion_products (promotion_id, product_id) VALUES (@promotionId, @productId)",
                    connection);
                command.Parameters.AddWithValue("@promotionId", promotionId);
                command.Parameters.AddWithValue("@productId", productId);
                command.ExecuteNonQuery();
            }
            return Ok("Product added to promotion successfully.");
        }

        // Lấy sản phẩm trong chương trình khuyến mãi
        [HttpGet]
        [Route("promotions/{promotionId}/products")]
        public IHttpActionResult GetProductsByPromotion(int promotionId)
        {
            var products = new List<PromotionProduct>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM promotion_products WHERE promotion_id = @promotionId", connection);
                command.Parameters.AddWithValue("@promotionId", promotionId);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    products.Add(new PromotionProduct
                    {
                        Id = (int)reader["id"],
                        PromotionId = (int)reader["promotion_id"],
                        ProductId = (int)reader["product_id"]
                    });
                }
            }
            return Ok(products);
        }

        // Kiểm tra khuyến mãi cho sản phẩm cụ thể
        [HttpGet]
        [Route("promotions/active")]
        public IHttpActionResult CheckPromotion(int productId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT TOP 1 discount_percent FROM promotions p " +
                    "JOIN promotion_products pp ON p.id = pp.promotion_id " +
                    "WHERE pp.product_id = @productId AND p.start_date <= GETDATE() AND p.end_date >= GETDATE()",
                    connection);
                command.Parameters.AddWithValue("@productId", productId);
                var discount = command.ExecuteScalar();
                if (discount != null)
                {
                    return Ok(new { ProductId = productId, DiscountPercent = (decimal)discount });
                }
            }
            return Ok(new { ProductId = productId, DiscountPercent = 0m });
        }

        // Xác thực sản phẩm thông qua dịch vụ quản lý sản phẩm
        private bool ValidateProduct(int productId, HttpRequestMessage request)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44361/"); // URL dịch vụ quản lý sản phẩm
                try
                {
                    if (request.Headers.Authorization == null || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
                    {
                        throw new Exception("Authorization header is missing or invalid.");
                    }

                    var jwtToken = request.Headers.Authorization.Parameter;
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = client.GetAsync($"products/{productId}").Result;

                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
