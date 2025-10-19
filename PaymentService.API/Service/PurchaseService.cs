using Microsoft.EntityFrameworkCore;
using PaymentService.API.Models.Entity;
using PaymentService.API.Repository;

namespace PaymentService.API.Service
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;

        public PurchaseService(IPurchaseRepository purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
        }

        public async Task<List<Purchase>> GetAllPurchasesAsync()
        {
            return await _purchaseRepository.GetAllPurchasesAsync();
        }

        public async Task<List<Purchase>> GetPurchasesByUserIdAsync(int userId)
        {
            return await _purchaseRepository.GetPurchasesByUserIdAsync(userId);
        }
    }
}