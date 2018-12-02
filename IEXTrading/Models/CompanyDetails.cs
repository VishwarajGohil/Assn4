using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IEXTrading.Models
{
    public class CompanyDetails
    {
        public decimal revenuePerShare { get; set; }
        public decimal returnOnEquity { get; set; }
        public decimal latestEPS { get; set; }
        public decimal latestPrice { get; set; }
        public decimal week52change { get; set; }
        public decimal variance { get; set; }
        public decimal PTE { get; set; }
        public string symbol { get; set; }
        public string companyName { get; set; }

    }
}
