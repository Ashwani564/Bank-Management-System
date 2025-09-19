# Bank Management System

<img width="984" height="880" alt="Screenshot 2025-09-18 at 10 51 43 PM" src="https://github.com/user-attachments/assets/79f3bee6-cd3e-43fb-b653-97fff95e72f7" />
A simpleC# console application for managing bank accounts with database integration.

## What it is
A basic banking system that allows users to:
- Create and manage bank accounts
- Deposit, withdraw, and transfer money
- View transaction history
- Admin panel for system management

## Technologies Used
- **C# (.NET 9.0)**
- **Entity Framework Core** - Database operations
- **PostgreSQL** - Database storage
- **BCrypt.Net** - Password encryption

## How to Run
### Prerequisites
1. **Install PostgreSQL**: Download and install from [postgresql.org](https://www.postgresql.org/download/)
2. **Create Database**: Run `setup.sql` script or create database manually:
   ```sql
   CREATE DATABASE bankdb;
   ```
3. **Update Connection**: Edit connection string in `Data/BankContext.cs`:
   ```csharp
   Host=localhost;Database=bankdb;Username=postgres;Password=your_password
   ```

### Run Application
1. **Navigate to project directory** in terminal
2. **Run**: `dotnet restore`
3. **Run**: `dotnet build`
4. **Run**: `dotnet run`

## Test Accounts
- **ACC001**: PIN 1234 (John Doe - $5,000)
- **ACC002**: PIN 5678 (Jane Smith - $2,500)
- **ACC003**: PIN 9876 (Bob Johnson - $10,000)

Admin password: `admin123`
