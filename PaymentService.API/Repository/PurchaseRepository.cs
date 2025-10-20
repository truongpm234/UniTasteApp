using Microsoft.EntityFrameworkCore;
using PaymentService.API.Data.DBContext;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Repository
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly Exe201PaymentServiceDbContext _context;

        public PurchaseRepository(Exe201PaymentServiceDbContext context)
        {
            _context = context;
        }

        public async Task<List<Purchase>> GetAllPurchasesAsync()
        {
            return await _context.Purchases
                .Include(x => x.ServicePackage)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Purchase>> GetPurchasesByUserIdAsync(int userId)
        {
            return await _context.Purchases
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsStatus(int userId)
        {
            var hasActive = _context.PaymentTransactions
                .Any(p => p.UserId == userId && p.Status == "Active");
            return hasActive;
        }
        public async Task<Purchase?> GetPurchaseByOrderCodeAsync(long orderCode)
        {
            return await _context.Purchases.FirstOrDefaultAsync(x => x.Description.Contains(orderCode.ToString()));
        }

        public async Task UpdatePurchaseAsync(Purchase purchase)
        {
            _context.Purchases.Update(purchase);
            await _context.SaveChangesAsync();
        }

    }
}
