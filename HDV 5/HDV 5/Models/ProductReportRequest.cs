using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HDV_5.Models
{
    public class ProductReportRequest
    {
        public int ProductId { get; set; }
        public int OrderReportId { get; set; }
        public decimal Cost { get; set; }

        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }

}