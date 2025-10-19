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
    }
}
