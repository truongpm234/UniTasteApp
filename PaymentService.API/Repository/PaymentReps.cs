using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentService.API.Data.DBContext;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Repositories
{
    public class PaymentReps : IPaymentReps
    {
        private readonly Exe201PaymentServiceDbContext _context;

        public PaymentReps(Exe201PaymentServiceDbContext context)
        {
            _context = context;
        }

        public async Task AddTransactionAsync(PaymentTransaction entity)
        {
            entity.CreatedAt = DateTime.UtcNow;

            _context.PaymentTransactions.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task AddPurchaseAsync(Purchase p)
        {
            p.CreatedAt = DateTime.UtcNow;
            await _context.Purchases.AddAsync(p);
            await _context.SaveChangesAsync();
        }
<<<<<<< HEAD
        public async Task<List<PaymentTransaction>> GetAllTransactionsAsync()
        {
            return await _context.PaymentTransactions.ToListAsync();
        }

=======
>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0
    }
}
