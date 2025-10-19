using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentService.API.Data.DBContext;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Repositories
{
    public class PaymentRepsitory : IPaymentRepsitory
    {
        private readonly Exe201PaymentServiceDbContext _context;

        public PaymentRepsitory(Exe201PaymentServiceDbContext context)
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

        public async Task<List<ServicePackage>> GetAllServicePackagesAsync()
        {
            return await _context.ServicePackages.ToListAsync();
        }

        public async Task<int> GetServicePackageByIdAsync(int packageId)
        {
            var package = await _context.ServicePackages.FirstOrDefaultAsync(p => p.ServicePackageId == packageId);
            if (package == null)
            {
                throw new Exception("Service package not found");
            }
            return package.ServicePackageId;
        }

    }
}
