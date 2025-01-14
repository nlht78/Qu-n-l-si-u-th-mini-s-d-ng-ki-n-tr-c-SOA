using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PromotionService.Models
{
    public class Promotion
    {
        public int Id { get; set; }                  // ID của chương trình khuyến mãi
        public string Name { get; set; }            // Tên chương trình khuyến mãi
        public string Description { get; set; }     // Mô tả chương trình
        public decimal DiscountPercent { get; set; } // Phần trăm giảm giá
        public DateTime StartDate { get; set; }     // Ngày bắt đầu
        public DateTime EndDate { get; set; }       // Ngày kết thúc
        public DateTime CreatedAt { get; set; }     // Ngày tạo
        public DateTime UpdatedAt { get; set; }     // Ngày cập nhật
    }
}