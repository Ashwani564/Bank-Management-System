using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankManagementSystem.Data;
using BankManagementSystem.Models;

namespace BankManagementSystem.Services
{
    public class TransactionService
    {
        private readonly BankContext _context;

        public TransactionService(BankContext context)
        {
            _context = context;
        }

        // Generate unique transaction number
        private async Task<string> GenerateTransactionNumberAsync()
        {
            var lastTransaction = await _context.Transactions
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();
            
            var nextId = (lastTransaction?.Id ?? 0) + 1;
            return $"TXN{nextId:D6}";
        }

        // Deposit money
        public async Task<(bool Success, string Message, Transaction? Transaction)> DepositAsync(string accountNumber, decimal amount, string? description = null)
        {
            if (amount <= 0)
                return (false, "Deposit amount must be greater than zero.", null);

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.IsActive);
            
            if (account == null)
                return (false, "Account not found or inactive.", null);

            var transaction = new Transaction
            {
                AccountId = account.Id,
                TransactionNumber = await GenerateTransactionNumberAsync(),
                Type = TransactionType.Deposit,
                Amount = amount,
                BalanceAfter = account.Balance + amount,
                Description = description ?? "Cash deposit",
                TransactionDate = DateTime.Now
            };

            account.Balance += amount;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return (true, $"Successfully deposited ${amount:F2}. New balance: ${account.Balance:F2}", transaction);
        }

        // Withdraw money
        public async Task<(bool Success, string Message, Transaction? Transaction)> WithdrawAsync(string accountNumber, decimal amount, string? description = null)
        {
            if (amount <= 0)
                return (false, "Withdrawal amount must be greater than zero.", null);

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.IsActive);
            
            if (account == null)
                return (false, "Account not found or inactive.", null);

            if (account.Balance < amount)
                return (false, "Insufficient funds.", null);

            var transaction = new Transaction
            {
                AccountId = account.Id,
                TransactionNumber = await GenerateTransactionNumberAsync(),
                Type = TransactionType.Withdrawal,
                Amount = amount,
                BalanceAfter = account.Balance - amount,
                Description = description ?? "Cash withdrawal",
                TransactionDate = DateTime.Now
            };

            account.Balance -= amount;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return (true, $"Successfully withdrew ${amount:F2}. New balance: ${account.Balance:F2}", transaction);
        }

        // Transfer money
        public async Task<(bool Success, string Message, Transaction? FromTransaction, Transaction? ToTransaction)> TransferAsync(
            string fromAccountNumber, string toAccountNumber, decimal amount, string? description = null)
        {
            if (amount <= 0)
                return (false, "Transfer amount must be greater than zero.", null, null);

            if (fromAccountNumber == toAccountNumber)
                return (false, "Cannot transfer to the same account.", null, null);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var fromAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == fromAccountNumber && a.IsActive);
                
                var toAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == toAccountNumber && a.IsActive);

                if (fromAccount == null)
                    return (false, "Source account not found or inactive.", null, null);

                if (toAccount == null)
                    return (false, "Destination account not found or inactive.", null, null);

                if (fromAccount.Balance < amount)
                    return (false, "Insufficient funds in source account.", null, null);

                var fromTransaction = new Transaction
                {
                    AccountId = fromAccount.Id,
                    TransactionNumber = await GenerateTransactionNumberAsync(),
                    Type = TransactionType.Transfer,
                    Amount = amount,
                    BalanceAfter = fromAccount.Balance - amount,
                    Description = description ?? $"Transfer to {toAccount.AccountHolderName}",
                    ToAccountNumber = toAccountNumber,
                    TransactionDate = DateTime.Now
                };

                var toTransaction = new Transaction
                {
                    AccountId = toAccount.Id,
                    TransactionNumber = await GenerateTransactionNumberAsync(),
                    Type = TransactionType.Transfer,
                    Amount = amount,
                    BalanceAfter = toAccount.Balance + amount,
                    Description = description ?? $"Transfer from {fromAccount.AccountHolderName}",
                    ToAccountNumber = fromAccountNumber,
                    TransactionDate = DateTime.Now
                };

                fromAccount.Balance -= amount;
                toAccount.Balance += amount;

                _context.Transactions.AddRange(fromTransaction, toTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Successfully transferred ${amount:F2} from {fromAccountNumber} to {toAccountNumber}.", fromTransaction, toTransaction);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return (false, "Transfer failed. Please try again.", null, null);
            }
        }

        // Get transaction history for an account
        public async Task<List<Transaction>> GetTransactionHistoryAsync(string accountNumber, int? limit = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.Account.AccountNumber == accountNumber);

            if (fromDate.HasValue)
                query = query.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.TransactionDate <= toDate.Value);

            query = query.OrderByDescending(t => t.TransactionDate);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            return await query.ToListAsync();
        }

        // Get all transactions (for admin)
        public async Task<List<Transaction>> GetAllTransactionsAsync(int? limit = null)
        {
            var query = _context.Transactions
                .Include(t => t.Account)
                .OrderByDescending(t => t.TransactionDate);

            if (limit.HasValue)
                return await query.Take(limit.Value).ToListAsync();

            return await query.ToListAsync();
        }

        // Get transactions by type
        public async Task<List<Transaction>> GetTransactionsByTypeAsync(TransactionType type, int? limit = null)
        {
            var query = _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.Type == type)
                .OrderByDescending(t => t.TransactionDate);

            if (limit.HasValue)
                return await query.Take(limit.Value).ToListAsync();

            return await query.ToListAsync();
        }

        // Get account balance
        public async Task<decimal?> GetAccountBalanceAsync(string accountNumber)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.IsActive);
            
            return account?.Balance;
        }
    }
}
