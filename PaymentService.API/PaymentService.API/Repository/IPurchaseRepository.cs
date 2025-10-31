using Microsoft.EntityFrameworkCore;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Repository
{
    public interface IPurchaseRepository
    {
        Task<List<Purchase>> GetAllPurchasesAsync();
        Task<List<Purchase>> GetPurchasesByUserIdAsync(int userId);
        Task<bool> IsStatus(int userId);
        Task<Purchase?> GetPurchaseByOrderCodeAsync(long orderCode);
        Task UpdatePurchaseAsync(Purchase purchase);
        
    }
}