using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace VaultTrack.Models
{
    [Table("user_balances")]
    public class UserBalance : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("balance_amount")]
        public decimal BalanceAmount { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
} 