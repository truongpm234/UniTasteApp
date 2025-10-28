using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentService.API.Data.DBContext;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly Exe201PaymentServiceDbContext _context;

        public PaymentRepository(Exe201PaymentServiceDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentTransaction> AddTransactionAsync(PaymentTransaction entity)
        {
            entity.CreatedAt = DateTime.UtcNow;

            _context.PaymentTransactions.Add(entity);

            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Purchase> AddPurchaseAsync(Purchase p)
        {
            p.CreatedAt = DateTime.UtcNow;
            await _context.Purchases.AddAsync(p);
            await _context.SaveChangesAsync();
            return p;
        }

        public async Task<IEnumerable<PaymentTransaction>> GetAllTransactionsAsync()
        {
            return await _context.PaymentTransactions
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PaymentTransaction?> GetTransactionByOrderCodeAsync(long orderCode)
        {
            return await _context.PaymentTransactions.FirstOrDefaultAsync(x => x.OrderCode == orderCode);
        }

        public async Task UpdateTransactionAsync(PaymentTransaction entity)
        {
            _context.PaymentTransactions.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountSuccessTransactionsAsync()
        {
            return await _context.PaymentTransactions.CountAsync(t => t.Status == "Success");
        }

        public async Task<int> CountCancelTransactionsAsync()
        {
            return await _context.PaymentTransactions.CountAsync(t => t.Status == "Cancel");
        }

        public async Task<double> TotalAmountOfSuccessfulTransactionsAsync()
        {
            return (double)await _context.PaymentTransactions
                .Where(t => t.Status == "Success" || t.Status == "Active")
                .SumAsync(t => t.Amount);
        }
    }
}
