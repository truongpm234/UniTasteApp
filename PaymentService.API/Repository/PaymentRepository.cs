using Microsoft.EntityFrameworkCore;
using PaymentService.API.Data.DBContext;
using PaymentService.API.Models.DTO;
using PaymentService.API.Models.Entity;
using System;
using System.Threading.Tasks;

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

        public async Task<int> CountPendingTransactionsAsync()
        {
            return await _context.PaymentTransactions.CountAsync(t => t.Status == "Pending");
        }       
        public async Task<double> TotalAmountOfSuccessfulTransactionsAsync()
        {
            return (double)await _context.PaymentTransactions
                .Where(t => t.Status == "Success" || t.Status == "Active")
                .SumAsync(t => t.Amount);
        }
        public async Task<int> CountAmountOfPaymentTransactionAsync()
        {
            return await _context.PaymentTransactions.CountAsync();
        }
        public async Task<List<DailyRevenueDto>> GetRevenueByDayLast7DaysAsync()
        {
            var fromDate = DateTime.UtcNow.Date.AddDays(-6); // 7 ngày: hôm nay và 6 ngày trước
            var toDate = DateTime.UtcNow.Date.AddDays(1); // đến hết hôm nay

            // Lấy giao dịch trong 7 ngày gần nhất, Status Success/Active, Amount != null
            var data = await _context.PaymentTransactions
                .Where(t =>
                    (t.Status == "Success" || t.Status == "Active") &&
                    t.CreatedAt != null &&
                    t.CreatedAt >= fromDate && t.CreatedAt < toDate &&
                    t.Amount != null)
                .GroupBy(t => t.CreatedAt.Value.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(x => x.Amount ?? 0)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Bảo đảm trả đủ 7 ngày (kể cả ngày không có giao dịch)
            var results = new List<DailyRevenueDto>();
            for (int i = 0; i < 7; i++)
            {
                var d = fromDate.AddDays(i);
                var found = data.FirstOrDefault(x => x.Date.Date == d.Date);
                results.Add(new DailyRevenueDto
                {
                    Date = d,
                    TotalRevenue = found?.TotalRevenue ?? 0
                });
            }
            return results;
        }

    }
}
