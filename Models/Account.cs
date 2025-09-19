using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankManagementSystem.Models
{
    public enum AccountType
    {
        Savings,
        Checking,
        Business,
        FixedDeposit
    }

    public class Account
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string AccountHolderName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;
        
        public AccountType AccountType { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal Balance { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public bool IsActive { get; set; } = true;
        
        [Required]
        [MaxLength(4)]
        public string PIN { get; set; } = string.Empty; // Hashed PIN
        
        // Navigation property
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
