using System.Collections.Generic;

namespace HDV_4.Models
{
    public class OrderItemRequest
    {
        public int OrderId { get; set; } // ID đơn hàng
        public List<OrderProduct> Products { get; set; } // Danh sách sản phẩm
    }

    public class OrderProduct
    {
        public int ProductId { get; set; } // ID sản phẩm
        public string ProductName { get; set; } // Tên sản phẩm
        public int Quantity { get; set; } // Số lượng
        public decimal UnitPrice { get; set; } // Giá mỗi sản phẩm
    }
}
