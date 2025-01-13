using System;
using SQLite;

namespace MoneyTracks.Models
{
    public class Debt
    {
        [PrimaryKey, AutoIncrement]
        public int DebtId { get; set; }

        public string Title { get; set; } // Title of the debt
        public string Source { get; set; } // Source of the debt (e.g., loan, borrowed)
        public double Amount { get; set; } // Debt amount
        public DateTime DueDate { get; set; } // Repayment deadline
        public bool IsCleared { get; set; } = false; // Status of the debt (default: not cleared)
        public string Notes { get; set; } // Optional notes
        public string Tags { get; set; } // Tags for categorization
    }
}
