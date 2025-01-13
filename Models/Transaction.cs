using SQLite;

namespace MoneyTracks.Models
{
    [Table("Transaction")]
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int TransactionId { get; set; }

        public string Title { get; set; }
        public string Type { get; set; } // Credit, Debit, Debt
        public double Amount { get; set; }
        public string Tags { get; set; } // Comma-separated custom tags
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public bool IsPendingDebt { get; set; } // True if it's a pending debt
    }
}
