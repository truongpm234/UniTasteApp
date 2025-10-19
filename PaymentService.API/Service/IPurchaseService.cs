using PaymentService.API.Models.Entity;

namespace PaymentService.API.Service
{
    public interface IPurchaseService
    {
        Task<List<Purchase>> GetAllPurchasesAsync();
        Task<List<Purchase>> GetPurchasesByUserIdAsync(int userId);
        
    }
}