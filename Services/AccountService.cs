using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankManagementSystem.Data;
using BankManagementSystem.Models;

namespace BankManagementSystem.Services
{
    public class AccountService
    {
        private readonly BankContext _context;

        public AccountService(BankContext context)
        {
            _context = context;
        }

        // Create a new account
        public async Task<Account> CreateAccountAsync(Account account)
        {
            // Generate unique account number
            var lastAccount = await _context.Accounts
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();
            
            var nextId = (lastAccount?.Id ?? 0) + 1;
            account.AccountNumber = $"ACC{nextId:D3}";
            
            // Hash the PIN
            account.PIN = BCrypt.Net.BCrypt.HashPassword(account.PIN);
            
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        // Get all accounts
        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _context.Accounts
                .Include(a => a.Transactions)
                .Where(a => a.IsActive)
                .ToListAsync();
        }

        // Get account by account number
        public async Task<Account?> GetAccountByNumberAsync(string accountNumber)
        {
            return await _context.Accounts
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.IsActive);
        }

        // Authenticate account with PIN
        public async Task<Account?> AuthenticateAsync(string accountNumber, string pin)
        {
            var account = await GetAccountByNumberAsync(accountNumber);
            if (account == null) return null;

            if (BCrypt.Net.BCrypt.Verify(pin, account.PIN))
                return account;
            
            return null;
        }

        // Update account
        public async Task<Account?> UpdateAccountAsync(string accountNumber, Account updatedAccount)
        {
            var account = await GetAccountByNumberAsync(accountNumber);
            if (account == null) return null;

            account.AccountHolderName = updatedAccount.AccountHolderName;
            account.Email = updatedAccount.Email;
            account.PhoneNumber = updatedAccount.PhoneNumber;
            account.Address = updatedAccount.Address;
            
            if (!string.IsNullOrEmpty(updatedAccount.PIN))
            {
                account.PIN = BCrypt.Net.BCrypt.HashPassword(updatedAccount.PIN);
            }

            await _context.SaveChangesAsync();
            return account;
        }

        // Close account (soft delete)
        public async Task<bool> CloseAccountAsync(string accountNumber)
        {
            var account = await GetAccountByNumberAsync(accountNumber);
            if (account == null) return false;

            account.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // Get accounts by type
        public async Task<List<Account>> GetAccountsByTypeAsync(AccountType type)
        {
            return await _context.Accounts
                .Where(a => a.AccountType == type && a.IsActive)
                .ToListAsync();
        }

        // Search accounts
        public async Task<List<Account>> SearchAccountsAsync(string searchTerm)
        {
            return await _context.Accounts
                .Where(a => a.IsActive && 
                           (a.AccountHolderName.Contains(searchTerm) || 
                            a.Email.Contains(searchTerm) ||
                            a.AccountNumber.Contains(searchTerm)))
                .ToListAsync();
        }
    }
}
