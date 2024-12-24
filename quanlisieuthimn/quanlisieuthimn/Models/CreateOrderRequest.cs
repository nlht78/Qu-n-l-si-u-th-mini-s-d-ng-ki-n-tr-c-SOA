namespace quanlisieuthimn.Models
{
    public class CreateOrderRequest
    {
        public Order Order { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
