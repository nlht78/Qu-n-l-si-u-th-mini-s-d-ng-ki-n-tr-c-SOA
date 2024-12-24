namespace quanlisieuthimn.Models
{
    public class OrderReportRequest
    {
        public int OrderId { get; set; }
        public decimal TotalCost { get; set; }

        public List<ProductReportRequest> Products { get; set; }
    }
}
