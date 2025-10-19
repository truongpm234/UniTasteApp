using PaymentService.API.Models.Entity;

namespace PaymentService.API.Service
{
    public interface IPaymentService
    {
        Task<PaymentTransaction> AddTransactionAsync(PaymentTransaction entity);
        Task<Purchase> AddPurchaseAsync(Purchase p);
        Task<IEnumerable<PaymentTransaction>> GetAllTransactionsAsync();
    }
}