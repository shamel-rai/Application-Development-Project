using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTracks.Models
{
    public class DashboardSummary
    {
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal TotalDebts { get; set; }
        public decimal ClearedDebts { get; set; }
        public decimal RemainingDebts { get; set; }

    }
}
