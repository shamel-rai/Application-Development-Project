using SQLite;

public class Debt
{
    [PrimaryKey, AutoIncrement]
    public int DebtId { get; set; }
    public string Title { get; set; }
    public string Source { get; set; }
    public decimal TotalAmount { get; set; } // Changed to decimal for financial precision
    public decimal RemainingAmount { get; set; } // Changed to decimal for compatibility
    public DateTime DueDate { get; set; }
    public bool IsCleared { get; set; } = false;
    public string Notes { get; set; }
    public string Tags { get; set; }
}
