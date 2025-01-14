using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PromotionService.Models
{
    public class PromotionProduct
    {
        public int Id { get; set; }              // ID của liên kết chương trình khuyến mãi - sản phẩm
        public int PromotionId { get; set; }    // ID chương trình khuyến mãi
        public int ProductId { get; set; }      // ID sản phẩm (tham chiếu từ dịch vụ sản phẩm)
    }
}