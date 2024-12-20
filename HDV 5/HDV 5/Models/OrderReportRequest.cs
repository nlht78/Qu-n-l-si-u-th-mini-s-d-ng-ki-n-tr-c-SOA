using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HDV_5.Models
{
    public class OrderReportRequest
    {
        public int OrderId { get; set; }
        public decimal TotalCost { get; set; }
    }

}