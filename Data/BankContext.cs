using System;
using Microsoft.EntityFrameworkCore;
using BankManagementSystem.Models;

namespace BankManagementSystem.Data
{
    public class BankContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Default connection string - update with your PostgreSQL server details
            var connectionString = "Host=localhost;Database=bankdb;Username=postgres;Password=password";
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Account configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.AccountNumber).IsRequired().HasMaxLength(20);
                entity.HasIndex(a => a.AccountNumber).IsUnique();
                entity.Property(a => a.AccountHolderName).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Email).IsRequired();
                entity.Property(a => a.PhoneNumber).IsRequired();
                entity.Property(a => a.Address).IsRequired().HasMaxLength(200);
                entity.Property(a => a.Balance).HasColumnType("decimal(18,2)");
                entity.Property(a => a.PIN).IsRequired().HasMaxLength(100); // For hashed PIN
            });

            // Transaction configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.TransactionNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(t => t.TransactionNumber).IsUnique();
                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
                entity.Property(t => t.BalanceAfter).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Description).HasMaxLength(500);
                entity.Property(t => t.ToAccountNumber).HasMaxLength(20);
                
                // Foreign key relationship
                entity.HasOne(t => t.Account)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(t => t.AccountId);
            });

            // Seed data
            var hashedPIN1 = BCrypt.Net.BCrypt.HashPassword("1234");
            var hashedPIN2 = BCrypt.Net.BCrypt.HashPassword("5678");
            var hashedPIN3 = BCrypt.Net.BCrypt.HashPassword("9876");

            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    Id = 1,
                    AccountNumber = "ACC001",
                    AccountHolderName = "John Doe",
                    Email = "john.doe@email.com",
                    PhoneNumber = "555-0101",
                    Address = "123 Main St, City, State 12345",
                    AccountType = AccountType.Savings,
                    Balance = 5000.00m,
                    CreatedDate = DateTime.Now.AddMonths(-6),
                    IsActive = true,
                    PIN = hashedPIN1
                },
                new Account
                {
                    Id = 2,
                    AccountNumber = "ACC002",
                    AccountHolderName = "Jane Smith",
                    Email = "jane.smith@email.com",
                    PhoneNumber = "555-0102",
                    Address = "456 Oak Ave, City, State 12345",
                    AccountType = AccountType.Checking,
                    Balance = 2500.50m,
                    CreatedDate = DateTime.Now.AddMonths(-3),
                    IsActive = true,
                    PIN = hashedPIN2
                },
                new Account
                {
                    Id = 3,
                    AccountNumber = "ACC003",
                    AccountHolderName = "Bob Johnson",
                    Email = "bob.johnson@email.com",
                    PhoneNumber = "555-0103",
                    Address = "789 Pine Rd, City, State 12345",
                    AccountType = AccountType.Business,
                    Balance = 10000.00m,
                    CreatedDate = DateTime.Now.AddMonths(-1),
                    IsActive = true,
                    PIN = hashedPIN3
                }
            );

            modelBuilder.Entity<Transaction>().HasData(
                new Transaction
                {
                    Id = 1,
                    AccountId = 1,
                    TransactionNumber = "TXN001",
                    Type = TransactionType.Deposit,
                    Amount = 1000.00m,
                    BalanceAfter = 5000.00m,
                    Description = "Initial deposit",
                    TransactionDate = DateTime.Now.AddDays(-30)
                },
                new Transaction
                {
                    Id = 2,
                    AccountId = 2,
                    TransactionNumber = "TXN002",
                    Type = TransactionType.Deposit,
                    Amount = 2500.50m,
                    BalanceAfter = 2500.50m,
                    Description = "Initial deposit",
                    TransactionDate = DateTime.Now.AddDays(-90)
                }
            );
        }
    }
}
