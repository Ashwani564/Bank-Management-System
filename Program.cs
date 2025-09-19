using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankManagementSystem.Data;
using BankManagementSystem.Models;
using BankManagementSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace BankManagementSystem
{
    class Program
    {
        private static AccountService? _accountService;
        private static TransactionService? _transactionService;
        private static Account? _currentAccount;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Welcome to ABC Bank Management System ===\n");

            // Initialize database and services
            using var context = new BankContext();
            await context.Database.EnsureCreatedAsync();
            
            _accountService = new AccountService(context);
            _transactionService = new TransactionService(context);

            await ShowMainMenu();
        }

        static async Task ShowMainMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Main Menu ===");
                Console.WriteLine("1. Account Login");
                Console.WriteLine("2. Create New Account");
                Console.WriteLine("3. Admin Panel");
                Console.WriteLine("4. Exit");
                Console.Write("\nEnter your choice (1-4): ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await AccountLogin();
                        break;
                    case "2":
                        await CreateAccount();
                        break;
                    case "3":
                        await AdminPanel();
                        break;
                    case "4":
                        Console.WriteLine("Thank you for using ABC Bank Management System!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        static async Task AccountLogin()
        {
            Console.WriteLine("\n=== Account Login ===");
            Console.Write("Enter Account Number: ");
            var accountNumber = Console.ReadLine();
            
            Console.Write("Enter PIN: ");
            var pin = ReadPassword();

            if (string.IsNullOrWhiteSpace(accountNumber) || string.IsNullOrWhiteSpace(pin))
            {
                Console.WriteLine("Account number and PIN are required.");
                return;
            }

            _currentAccount = await _accountService!.AuthenticateAsync(accountNumber, pin);
            
            if (_currentAccount == null)
            {
                Console.WriteLine("Invalid account number or PIN.");
                return;
            }

            Console.WriteLine($"\nWelcome, {_currentAccount.AccountHolderName}!");
            await ShowAccountMenu();
        }

        static async Task ShowAccountMenu()
        {
            while (_currentAccount != null)
            {
                Console.WriteLine($"\n=== Account Menu - {_currentAccount.AccountHolderName} ===");
                Console.WriteLine($"Account: {_currentAccount.AccountNumber} | Balance: ${_currentAccount.Balance:F2}");
                Console.WriteLine("\n1. Check Balance");
                Console.WriteLine("2. Deposit Money");
                Console.WriteLine("3. Withdraw Money");
                Console.WriteLine("4. Transfer Money");
                Console.WriteLine("5. Transaction History");
                Console.WriteLine("6. Update Profile");
                Console.WriteLine("7. Logout");
                Console.Write("\nEnter your choice (1-7): ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await CheckBalance();
                        break;
                    case "2":
                        await DepositMoney();
                        break;
                    case "3":
                        await WithdrawMoney();
                        break;
                    case "4":
                        await TransferMoney();
                        break;
                    case "5":
                        await ViewTransactionHistory();
                        break;
                    case "6":
                        await UpdateProfile();
                        break;
                    case "7":
                        _currentAccount = null;
                        Console.WriteLine("Logged out successfully.");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        static async Task CreateAccount()
        {
            Console.WriteLine("\n=== Create New Account ===");
            
            Console.Write("Enter Full Name: ");
            var name = Console.ReadLine();
            
            Console.Write("Enter Email: ");
            var email = Console.ReadLine();
            
            Console.Write("Enter Phone Number: ");
            var phone = Console.ReadLine();
            
            Console.Write("Enter Address: ");
            var address = Console.ReadLine();
            
            Console.WriteLine("\nSelect Account Type:");
            Console.WriteLine("1. Savings");
            Console.WriteLine("2. Checking");
            Console.WriteLine("3. Business");
            Console.WriteLine("4. Fixed Deposit");
            Console.Write("Enter choice (1-4): ");
            var typeChoice = Console.ReadLine();
            
            Console.Write("Set 4-digit PIN: ");
            var pin = ReadPassword();
            
            Console.Write("Initial Deposit Amount: $");
            var initialDepositStr = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(pin) || pin.Length != 4 || !pin.All(char.IsDigit))
            {
                Console.WriteLine("All fields are required and PIN must be 4 digits.");
                return;
            }

            if (!decimal.TryParse(initialDepositStr, out var initialDeposit) || initialDeposit < 0)
            {
                Console.WriteLine("Invalid initial deposit amount.");
                return;
            }

            AccountType accountType = typeChoice switch
            {
                "1" => AccountType.Savings,
                "2" => AccountType.Checking,
                "3" => AccountType.Business,
                "4" => AccountType.FixedDeposit,
                _ => AccountType.Savings
            };

            var account = new Account
            {
                AccountHolderName = name,
                Email = email,
                PhoneNumber = phone,
                Address = address,
                AccountType = accountType,
                Balance = initialDeposit,
                PIN = pin
            };

            var createdAccount = await _accountService!.CreateAccountAsync(account);
            Console.WriteLine($"\nAccount created successfully!");
            Console.WriteLine($"Account Number: {createdAccount.AccountNumber}");
            Console.WriteLine($"Initial Balance: ${createdAccount.Balance:F2}");
        }

        static async Task CheckBalance()
        {
            // Refresh current account data
            _currentAccount = await _accountService!.GetAccountByNumberAsync(_currentAccount!.AccountNumber);
            Console.WriteLine($"\nCurrent Balance: ${_currentAccount!.Balance:F2}");
        }

        static async Task DepositMoney()
        {
            Console.Write("\nEnter deposit amount: $");
            if (!decimal.TryParse(Console.ReadLine(), out var amount))
            {
                Console.WriteLine("Invalid amount.");
                return;
            }

            Console.Write("Enter description (optional): ");
            var description = Console.ReadLine();

            var result = await _transactionService!.DepositAsync(_currentAccount!.AccountNumber, amount, description);
            Console.WriteLine($"\n{result.Message}");
            
            if (result.Success)
            {
                // Refresh current account data
                _currentAccount = await _accountService!.GetAccountByNumberAsync(_currentAccount.AccountNumber);
            }
        }

        static async Task WithdrawMoney()
        {
            Console.Write("\nEnter withdrawal amount: $");
            if (!decimal.TryParse(Console.ReadLine(), out var amount))
            {
                Console.WriteLine("Invalid amount.");
                return;
            }

            Console.Write("Enter description (optional): ");
            var description = Console.ReadLine();

            var result = await _transactionService!.WithdrawAsync(_currentAccount!.AccountNumber, amount, description);
            Console.WriteLine($"\n{result.Message}");
            
            if (result.Success)
            {
                // Refresh current account data
                _currentAccount = await _accountService!.GetAccountByNumberAsync(_currentAccount.AccountNumber);
            }
        }

        static async Task TransferMoney()
        {
            Console.Write("\nEnter destination account number: ");
            var toAccount = Console.ReadLine();
            
            Console.Write("Enter transfer amount: $");
            if (!decimal.TryParse(Console.ReadLine(), out var amount))
            {
                Console.WriteLine("Invalid amount.");
                return;
            }

            Console.Write("Enter description (optional): ");
            var description = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(toAccount))
            {
                Console.WriteLine("Destination account number is required.");
                return;
            }

            var result = await _transactionService!.TransferAsync(_currentAccount!.AccountNumber, toAccount, amount, description);
            Console.WriteLine($"\n{result.Message}");
            
            if (result.Success)
            {
                // Refresh current account data
                _currentAccount = await _accountService!.GetAccountByNumberAsync(_currentAccount.AccountNumber);
            }
        }

        static async Task ViewTransactionHistory()
        {
            Console.WriteLine("\n=== Transaction History ===");
            Console.Write("Enter number of recent transactions to show (default 10): ");
            var limitStr = Console.ReadLine();
            
            var limit = string.IsNullOrWhiteSpace(limitStr) ? 10 : 
                       (int.TryParse(limitStr, out var l) ? l : 10);

            var transactions = await _transactionService!.GetTransactionHistoryAsync(_currentAccount!.AccountNumber, limit);
            
            if (!transactions.Any())
            {
                Console.WriteLine("No transactions found.");
                return;
            }

            DisplayTransactions(transactions);
        }

        static async Task UpdateProfile()
        {
            Console.WriteLine("\n=== Update Profile ===");
            Console.WriteLine("Enter new details (press Enter to keep current value):");
            
            Console.Write($"Name [{_currentAccount!.AccountHolderName}]: ");
            var name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) _currentAccount.AccountHolderName = name;
            
            Console.Write($"Email [{_currentAccount.Email}]: ");
            var email = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(email)) _currentAccount.Email = email;
            
            Console.Write($"Phone [{_currentAccount.PhoneNumber}]: ");
            var phone = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(phone)) _currentAccount.PhoneNumber = phone;
            
            Console.Write($"Address [{_currentAccount.Address}]: ");
            var address = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(address)) _currentAccount.Address = address;
            
            Console.Write("New PIN (leave blank to keep current): ");
            var newPin = ReadPassword();

            if (!string.IsNullOrWhiteSpace(newPin))
            {
                if (newPin.Length != 4 || !newPin.All(char.IsDigit))
                {
                    Console.WriteLine("PIN must be 4 digits.");
                    return;
                }
                _currentAccount.PIN = newPin;
            }

            var updatedAccount = await _accountService!.UpdateAccountAsync(_currentAccount.AccountNumber, _currentAccount);
            if (updatedAccount != null)
            {
                Console.WriteLine("Profile updated successfully!");
                _currentAccount = updatedAccount;
            }
            else
            {
                Console.WriteLine("Failed to update profile.");
            }
        }

        static async Task AdminPanel()
        {
            Console.WriteLine("\n=== Admin Panel ===");
            Console.Write("Enter Admin Password: ");
            var password = ReadPassword();

            if (password != "admin123") // Simple admin authentication
            {
                Console.WriteLine("Invalid admin password.");
                return;
            }

            while (true)
            {
                Console.WriteLine("\n=== Admin Menu ===");
                Console.WriteLine("1. View All Accounts");
                Console.WriteLine("2. Search Account");
                Console.WriteLine("3. View All Transactions");
                Console.WriteLine("4. Account Statistics");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("\nEnter your choice (1-5): ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await ViewAllAccounts();
                        break;
                    case "2":
                        await SearchAccount();
                        break;
                    case "3":
                        await ViewAllTransactions();
                        break;
                    case "4":
                        await ShowAccountStatistics();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        static async Task ViewAllAccounts()
        {
            Console.WriteLine("\n=== All Accounts ===");
            var accounts = await _accountService!.GetAllAccountsAsync();
            DisplayAccounts(accounts);
        }

        static async Task SearchAccount()
        {
            Console.Write("\nEnter search term (name, email, or account number): ");
            var searchTerm = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Console.WriteLine("Search term cannot be empty.");
                return;
            }

            var accounts = await _accountService!.SearchAccountsAsync(searchTerm);
            Console.WriteLine($"\n=== Search Results for '{searchTerm}' ===");
            DisplayAccounts(accounts);
        }

        static async Task ViewAllTransactions()
        {
            Console.WriteLine("\n=== All Transactions ===");
            Console.Write("Enter number of recent transactions to show (default 20): ");
            var limitStr = Console.ReadLine();
            
            var limit = string.IsNullOrWhiteSpace(limitStr) ? 20 : 
                       (int.TryParse(limitStr, out var l) ? l : 20);

            var transactions = await _transactionService!.GetAllTransactionsAsync(limit);
            DisplayTransactions(transactions);
        }

        static async Task ShowAccountStatistics()
        {
            Console.WriteLine("\n=== Account Statistics ===");
            var accounts = await _accountService!.GetAllAccountsAsync();
            
            var totalAccounts = accounts.Count;
            var totalBalance = accounts.Sum(a => a.Balance);
            var savingsAccounts = accounts.Count(a => a.AccountType == AccountType.Savings);
            var checkingAccounts = accounts.Count(a => a.AccountType == AccountType.Checking);
            var businessAccounts = accounts.Count(a => a.AccountType == AccountType.Business);
            var fdAccounts = accounts.Count(a => a.AccountType == AccountType.FixedDeposit);

            Console.WriteLine($"Total Accounts: {totalAccounts}");
            Console.WriteLine($"Total Bank Balance: ${totalBalance:F2}");
            Console.WriteLine($"Savings Accounts: {savingsAccounts}");
            Console.WriteLine($"Checking Accounts: {checkingAccounts}");
            Console.WriteLine($"Business Accounts: {businessAccounts}");
            Console.WriteLine($"Fixed Deposit Accounts: {fdAccounts}");
        }

        static void DisplayAccounts(List<Account> accounts)
        {
            if (!accounts.Any())
            {
                Console.WriteLine("No accounts found.");
                return;
            }

            Console.WriteLine(new string('-', 120));
            Console.WriteLine($"{"Account#",-10} {"Name",-20} {"Email",-25} {"Type",-12} {"Balance",-15} {"Created",-12}");
            Console.WriteLine(new string('-', 120));
            
            foreach (var account in accounts)
            {
                Console.WriteLine($"{account.AccountNumber,-10} {account.AccountHolderName,-20} {account.Email,-25} {account.AccountType,-12} ${account.Balance,-14:F2} {account.CreatedDate:yyyy-MM-dd}");
            }
        }

        static void DisplayTransactions(List<Transaction> transactions)
        {
            if (!transactions.Any())
            {
                Console.WriteLine("No transactions found.");
                return;
            }

            Console.WriteLine(new string('-', 120));
            Console.WriteLine($"{"TXN#",-10} {"Account",-10} {"Type",-12} {"Amount",-12} {"Balance",-12} {"Date",-12} {"Description",-20}");
            Console.WriteLine(new string('-', 120));
            
            foreach (var txn in transactions)
            {
                Console.WriteLine($"{txn.TransactionNumber,-10} {txn.Account.AccountNumber,-10} {txn.Type,-12} ${txn.Amount,-11:F2} ${txn.BalanceAfter,-11:F2} {txn.TransactionDate:yyyy-MM-dd} {txn.Description,-20}");
            }
        }

        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter)
                {
                    password += keyInfo.KeyChar;
                    Console.Write("*");
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b");
                }
            } while (keyInfo.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }
    }
}
