using Microsoft.EntityFrameworkCore;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Repository
{
    public interface IPurchaseRepository
    {
        Task<List<Purchase>> GetAllPurchasesAsync();
        Task<List<Purchase>> GetPurchasesByUserIdAsync(int userId);
        
    }
}