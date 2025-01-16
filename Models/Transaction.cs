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

        public string Type { get; set; }

        public decimal Amount { get; set; }

        public string Tags { get; set; }

     
        public DateTime Date { get; set; }

        public string Notes { get; set; }

        public bool IsPendingDebt { get; set; }

        // Helper property to parse tags into a list
        public string[] ParsedTags => string.IsNullOrEmpty(Tags)
            ? Array.Empty<string>()
            : Tags.Split(',').Select(tag => tag.Trim()).ToArray();
    }
}
