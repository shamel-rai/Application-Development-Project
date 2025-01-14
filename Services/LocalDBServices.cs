using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoneyTracks.Models;

namespace MoneyTracks.Services
{
    public class LocalDbService
    {
        private const string DB_NAME = "MoneySpot.db";
        private readonly SQLiteAsyncConnection _connection;

        public LocalDbService()
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                DB_NAME);

            _connection = new SQLiteAsyncConnection(dbPath);

            // Ensure tables exist
            _connection.CreateTableAsync<User>().Wait();
            _connection.CreateTableAsync<Transaction>().Wait();
            _connection.CreateTableAsync<Debt>().Wait();
        }

        // ----------------------------------------------------------------
        //  User CRUD
        // ----------------------------------------------------------------
        public async Task<User> GetLoggedInUser()
        {
            // Just fetch the first user for demo
            return await _connection.Table<User>().FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetUsers() =>
            await _connection.Table<User>().ToListAsync();

        public async Task<User> GetByID(int id) =>
            await _connection.Table<User>().FirstOrDefaultAsync(x => x.UserId == id);

        public async Task Create(User user) =>
            await _connection.InsertAsync(user);

        public async Task Update(User user) =>
            await _connection.UpdateAsync(user);

        public async Task Delete(User user) =>
            await _connection.DeleteAsync(user);


        // ----------------------------------------------------------------
        //  Transaction CRUD
        // ----------------------------------------------------------------
        public async Task<List<Transaction>> GetTransactions() =>
            await _connection.Table<Transaction>().ToListAsync();

        public async Task<Transaction> GetTransactionById(int id) =>
            await _connection.Table<Transaction>().FirstOrDefaultAsync(t => t.TransactionId == id);

        public async Task CreateTransaction(Transaction transaction)
        {
            // If it's a direct debit, verify we have enough available cash
            if (transaction.Type == "Debit")
            {
                var availableBalance = await GetAvailableCash();
                if (transaction.Amount > availableBalance)
                {
                    throw new InvalidOperationException("Insufficient balance for this transaction.");
                }
            }

            // If it's a debt payment, handle with a dedicated method
            if (transaction.Type == "DebtPayment")
            {
                await CreateDebtPaymentTransaction(transaction);
            }
            else
            {
                // Insert normal credit/debit
                await _connection.InsertAsync(transaction);
            }
        }

        public async Task UpdateTransaction(Transaction transaction) =>
            await _connection.UpdateAsync(transaction);

        public async Task DeleteTransaction(Transaction transaction) =>
            await _connection.DeleteAsync(transaction);

        public async Task<List<Transaction>> GetTopTransactions(int count)
        {
            var transactions = await _connection.Table<Transaction>()
                .OrderByDescending(t => t.Amount)
                .Take(count)
                .ToListAsync();
            return transactions;
        }


        // ----------------------------------------------------------------
        //  Dashboard
        // ----------------------------------------------------------------
        public async Task<DashboardSummary> GetDashboardSummary()
        {
            var transactions = await GetTransactions();
            var debts = await GetAllDebts();

            return new DashboardSummary
            {
                TotalInflows = transactions.Where(t => t.Type == "Credit").Sum(t => t.Amount),
                TotalOutflows = transactions.Where(t => t.Type == "Debit").Sum(t => t.Amount),
                TotalDebts = debts.Sum(d => d.TotalAmount),
                ClearedDebts = debts.Where(d => d.IsCleared).Sum(d => d.TotalAmount),
                RemainingDebts = debts.Sum(d => d.RemainingAmount)
            };
        }

        public async Task<List<Debt>> GetPendingDebtsDashboard()
        {
            var debts = await _connection.Table<Debt>()
                .Where(d => !d.IsCleared)
                .OrderBy(d => d.DueDate)
                .ToListAsync();
            return debts;
        }


        // ----------------------------------------------------------------
        //  Debt CRUD
        // ----------------------------------------------------------------
        public async Task<List<Debt>> GetAllDebts() =>
            await _connection.Table<Debt>().ToListAsync();

        public async Task AddDebt(Debt debt)
        {
            // Initialize remaining amount
            debt.RemainingAmount = debt.TotalAmount;
            await _connection.InsertAsync(debt);
        }

        public async Task UpdateDebt(Debt debt)
        {
            await _connection.UpdateAsync(debt);
        }

        public async Task UpdateDebtAfterPayment(int debtId, decimal paymentAmount)
        {
            var debt = await _connection.Table<Debt>()
                .FirstOrDefaultAsync(d => d.DebtId == debtId);

            if (debt != null)
            {
                debt.RemainingAmount -= paymentAmount;
                if (debt.RemainingAmount <= 0)
                {
                    debt.RemainingAmount = 0;
                    debt.IsCleared = true;
                }
                await _connection.UpdateAsync(debt);
            }
        }

        public async Task DeleteDebt(Debt debt) =>
            await _connection.DeleteAsync(debt);


        // ----------------------------------------------------------------
        //  Handle Debt Payment Transaction
        // ----------------------------------------------------------------
        public async Task CreateDebtPaymentTransaction(Transaction transaction)
        {
            // If it references a specific debt by Title
            if (transaction.IsPendingDebt)
            {
                var debt = await _connection.Table<Debt>()
                    .FirstOrDefaultAsync(d => d.Title == transaction.Title);

                if (debt != null)
                {
                    // Decrease the debt
                    await UpdateDebtAfterPayment(debt.DebtId, transaction.Amount);
                }
            }

            // Finally insert the "DebtPayment" transaction
            await _connection.InsertAsync(transaction);
        }

        public async Task ClearDebtPartiallyOrFully(Debt debt)
        {
            var availableCash = await GetAvailableCash();
            if (availableCash <= 0)
            {
                throw new InvalidOperationException("Insufficient cash to clear the debt.");
            }

            if (availableCash >= debt.RemainingAmount)
            {
                // Clear fully
                availableCash -= debt.RemainingAmount;
                debt.RemainingAmount = 0;
                debt.IsCleared = true;
            }
            else
            {
                // Partially clear
                debt.RemainingAmount -= availableCash;
                debt.IsCleared = false;
                availableCash = 0;
            }

            await _connection.UpdateAsync(debt);
        }


        // ----------------------------------------------------------------
        //  Utilities
        // ----------------------------------------------------------------
        public async Task<string> GetCurrencySymbol()
        {
            var user = await GetLoggedInUser();
            return user?.PreferredCurrency switch
            {
                "USD" => "$",
                "EUR" => "€",
                "GBP" => "£",
                "JPY" => "¥",
                _ => "$" // default to USD
            };
        }

        public async Task<decimal> GetAvailableCash()
        {
            var transactions = await GetTransactions();

            var totalInflows = transactions
                .Where(t => t.Type == "Credit")
                .Sum(t => t.Amount);

            // Both "Debit" and "DebtPayment" are outflows
            var totalOutflows = transactions
                .Where(t => t.Type == "Debit" || t.Type == "DebtPayment")
                .Sum(t => t.Amount);

            return totalInflows - totalOutflows;
        }
    }
}
