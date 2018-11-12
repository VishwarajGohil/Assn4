using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IEXTrading.Models
{
    public class EPS
    {
        public float actualEPS { get; set; }
        public float consensusEPS { get; set; }
        public float estimatedEPS { get; set; }
        public string announceTime { get; set; }
        public float numberOfEstimates { get; set; }
        public float EPSSurpriseDollar { get; set; }
        public string EPSReportDate { get; set; }
        public string fiscalPeriod { get; set; }
        public string fiscalEndDate { get; set; }
        public float yearAgo { get; set; }
        public float yearAgoChangePercent { get; set; }
        public float estimatedChangePercent { get; set; }
        public string symbolId { get; set; }    
    } 
    
}
