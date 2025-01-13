using SQLite;
using System;
using System.Collections.Generic;
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
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DB_NAME);
            _connection = new SQLiteAsyncConnection(dbPath);

            _connection.CreateTableAsync<User>().Wait();
            _connection.CreateTableAsync<Transaction>().Wait();
            _connection.CreateTableAsync<Debt>().Wait();
        }

        // User CRUD
        public async Task<List<User>> GetUsers() => await _connection.Table<User>().ToListAsync();

        public async Task<User> GetByID(int id) => await _connection.Table<User>().FirstOrDefaultAsync(x => x.UserId == id);

        public async Task Create(User user) => await _connection.InsertAsync(user);

        public async Task Update(User user) => await _connection.UpdateAsync(user);

        public async Task Delete(User user) => await _connection.DeleteAsync(user);

        // Transaction CRUD
        public async Task<List<Transaction>> GetTransactions() => await _connection.Table<Transaction>().ToListAsync();

        public async Task<Transaction> GetTransactionById(int id) => await _connection.Table<Transaction>().FirstOrDefaultAsync(t => t.TransactionId == id);

        public async Task CreateTransaction(Transaction transaction) => await _connection.InsertAsync(transaction);

        public async Task UpdateTransaction(Transaction transaction) => await _connection.UpdateAsync(transaction);

        public async Task DeleteTransaction(Transaction transaction) => await _connection.DeleteAsync(transaction);


        // Debt CRUD
        public async Task<List<Debt>> GetAllDebts() => await _connection.Table<Debt>().ToListAsync();

        public async Task AddDebt(Debt debt) => await _connection.InsertAsync(debt);

        public async Task UpdateDebt(Debt debt) => await _connection.UpdateAsync(debt);

        public async Task DeleteDebt(Debt debt) => await _connection.DeleteAsync(debt);

        public async Task<List<Debt>> GetPendingDebts()
        {
            return await _connection.Table<Debt>()
                .Where(d => !d.IsCleared)
                .OrderBy(d => d.DueDate)
                .ToListAsync();
        }

        // Dashboard Methods
        public async Task<DashboardSummary> GetDashboardSummary()
        {
            var transactions = await GetTransactions();
            var debts = await GetAllDebts();

            return new DashboardSummary
            {
                TotalInflows = (decimal)transactions.Where(t => t.Type == "Credit").Sum(t => t.Amount),
                TotalOutflows = (decimal)transactions.Where(t => t.Type == "Debit").Sum(t => t.Amount),
                TotalDebts = (decimal)debts.Sum(d => d.Amount),
                ClearedDebts = (decimal)debts.Where(d => d.IsCleared).Sum(d => d.Amount),
                RemainingDebts = (decimal)debts.Where(d => !d.IsCleared).Sum(d => d.Amount)
            };
        }

        public async Task<List<Transaction>> GetTopTransactions(int count)
        {
            return await _connection.Table<Transaction>()
                .OrderByDescending(t => t.Amount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Debt>> GetPendingDebtsDashboard()
        {
            return await _connection.Table<Debt>()
                .Where(d => !d.IsCleared)
                .OrderBy(d => d.DueDate)
                .ToListAsync();
        }
    }
}
