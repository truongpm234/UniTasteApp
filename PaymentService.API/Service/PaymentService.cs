using Microsoft.EntityFrameworkCore;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepsitory _repository;

        public PaymentService(IPaymentRepsitory repository)
        {
            _repository = repository;
        }
        public async Task<PaymentTransaction> AddTransactionAsync(PaymentTransaction entity)
        {
            return await _repository.AddTransactionAsync(entity);
        }

        public async Task<Purchase> AddPurchaseAsync(Purchase p)
        {
            return await _repository.AddPurchaseAsync(p);
        }

        public async Task<IEnumerable<PaymentTransaction>> GetAllTransactionsAsync()
        {
            return await _repository.GetAllTransactionsAsync();
        }
    }
}
