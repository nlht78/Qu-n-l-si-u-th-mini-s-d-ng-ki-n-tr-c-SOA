using HDV_5.Models;
using HDV_5.Services;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;

namespace HDV_5.Controllers
{
    [JwtAuthentication] // Yêu cầu xác thực qua JWT
    public class ReportsController : ApiController
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ReportDBConnection"].ConnectionString;

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

        // POST /reports/products
        [HttpPost]
        [Route("reports/products")]
        public IHttpActionResult CreateProductReport(ProductReport report)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO product_reports (order_report_id, product_id, total_sold, revenue, cost) VALUES (@orderReportId, @productId, @totalSold, @revenue, @cost)", connection);
                command.Parameters.AddWithValue("@orderReportId", report.OrderReportId);
                command.Parameters.AddWithValue("@productId", report.ProductId);
                command.Parameters.AddWithValue("@totalSold", report.TotalSold);
                command.Parameters.AddWithValue("@revenue", report.Revenue);
                command.Parameters.AddWithValue("@cost", report.Cost);
                command.ExecuteNonQuery();
            }
            return Ok("Product report created successfully.");
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

        // Các API tương tự cho orders_reports...
    }
}
