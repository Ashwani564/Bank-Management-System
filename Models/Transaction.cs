using System;
using System.ComponentModel.DataAnnotations;

namespace BankManagementSystem.Models
{
    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Transfer,
        InterestCredit
    }

    public class Transaction
    {
        public int Id { get; set; }
        
        public int AccountId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;
        
        public TransactionType Type { get; set; }
        
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        public decimal BalanceAfter { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(20)]
        public string? ToAccountNumber { get; set; } // For transfers
        
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        
        // Navigation property
        public Account Account { get; set; } = null!;
    }
}
