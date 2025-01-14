namespace MoneyTracks.Models
{
    public class DashboardSummary
    {
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal TotalDebts { get; set; }
        public decimal ClearedDebts { get; set; }
        public decimal RemainingDebts { get; set; }

        public string FormatCurrency(decimal amount, string symbol)
        {
            return $"{symbol}{amount:N2}";
        }
    }
}
