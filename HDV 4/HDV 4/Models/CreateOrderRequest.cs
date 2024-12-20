using HDV_4.Models;
using System.Collections.Generic;

public class CreateOrderRequest
{
    public Order Order { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}
