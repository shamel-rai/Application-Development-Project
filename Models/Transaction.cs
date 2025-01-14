using SQLite;
using System;
using System.Linq;

namespace MoneyTracks.Models
{
    [Table("Transaction")]
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int TransactionId { get; set; }

        public string Title { get; set; }

        // "Credit", "Debit", "Debt", or "DebtPayment"
        public string Type { get; set; }

        public decimal Amount { get; set; }

        // Comma-separated custom tags
        public string Tags { get; set; }

        // Single date property for the transaction
        public DateTime Date { get; set; }

        public string Notes { get; set; }

        // True if this transaction is intended to reduce a debt (DebtPayment)
        public bool IsPendingDebt { get; set; }

        // Helper property to parse tags into a list
        public string[] ParsedTags => string.IsNullOrEmpty(Tags)
            ? Array.Empty<string>()
            : Tags.Split(',').Select(tag => tag.Trim()).ToArray();
    }
}
