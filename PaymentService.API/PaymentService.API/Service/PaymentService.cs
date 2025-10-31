using Microsoft.EntityFrameworkCore;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;

        public PaymentService(IPaymentRepository repository)
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
        public async Task<PaymentTransaction?> GetTransactionByOrderCodeAsync(long orderCode)
        {
            return await _repository.GetTransactionByOrderCodeAsync(orderCode);
        }

        public async Task UpdateTransactionAsync(PaymentTransaction entity)
        {
            await _repository.UpdateTransactionAsync(entity);
        }
        public async Task<int> CountSuccessTransactionsAsync()
        {
            return await _repository.CountSuccessTransactionsAsync();
        }
        public async Task<int> CountCancelTransactionsAsync()
        {
            return await _repository.CountSuccessTransactionsAsync();
        }
        public async Task<double> TotalAmountOfSuccessfulTransactionsAsync()
        {
            return await _repository.TotalAmountOfSuccessfulTransactionsAsync();
        }

        public async Task<int> CountPendingTransactionsAsync()
        {
            return await _repository.CountPendingTransactionsAsync();
        }

        public async Task<int> CountAmountOfPaymentTransactionAsync()
        {
            return await _repository.CountAmountOfPaymentTransactionAsync();
        }
    }
}
