using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaultTrack.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal MonthlyBudget { get; set; }
        public string? PasswordHash { get; set; }
    }
} 