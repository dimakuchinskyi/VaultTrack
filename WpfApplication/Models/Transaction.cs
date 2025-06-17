using System;
using Postgrest.Models;
using Postgrest.Attributes;

namespace VaultTrack.Models
{
    [Table("transactions")]
    public class Transaction : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("type")]
        public string? Type { get; set; } // "Income" or "Expense"

        [Column("category")]
        public string? Category { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("description")]
        public string? Description { get; set; }
    }
} 