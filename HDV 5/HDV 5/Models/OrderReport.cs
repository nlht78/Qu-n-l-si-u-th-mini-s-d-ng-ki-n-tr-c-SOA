using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HDV_5.Models
{
    public class OrderReport
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
    }
}