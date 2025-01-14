namespace quanlisieuthimn.Models
{
    public class OrderDetailsResponse
    {
        public Order Order { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
