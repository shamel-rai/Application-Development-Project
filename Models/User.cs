using SQLite;

namespace MoneyTracks.Models
{
    [Table("User")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PreferredCurrency { get; set; }
    }
}