using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HDV_3.Models
{
    public class Category
    {
        public int Id { get; set; } // ID loại sản phẩm
        public string Name { get; set; } // Tên loại sản phẩm
        public string Description { get; set; } // Mô tả (nếu cần)
        public DateTime CreatedAt { get; set; } // Ngày tạo
        public DateTime UpdatedAt { get; set; } // Ngày cập nhật
    }

}