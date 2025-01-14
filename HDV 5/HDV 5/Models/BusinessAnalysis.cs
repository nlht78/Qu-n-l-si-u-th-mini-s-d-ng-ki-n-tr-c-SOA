using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HDV_5.Models
{
    public class BusinessAnalysis
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUnitsSold { get; set; }
        public decimal AverageRevenuePerProduct { get; set; }
        public decimal AverageProfitPerProduct { get; set; }
    }
}