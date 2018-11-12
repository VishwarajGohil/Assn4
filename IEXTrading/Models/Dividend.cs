using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IEXTrading.Models
{
    public class Dividend
    {
        public string exDate { get; set; }
        public string paymentDate { get; set; }
        public string recordDate { get; set; }
        public string declaredDate { get; set; }
        public float amount { get; set; }
        public string flag { get; set; }
        public string type { get; set; }
        public string qualified { get; set; }
        public string indicated { get; set; }        
    }
}
