using Microsoft.EntityFrameworkCore;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Service
{
    public interface IPaymentService
    {
        Task<PaymentTransaction> AddTransactionAsync(PaymentTransaction entity);
        Task<Purchase> AddPurchaseAsync(Purchase p);
        Task<IEnumerable<PaymentTransaction>> GetAllTransactionsAsync();
        Task<PaymentTransaction?> GetTransactionByOrderCodeAsync(long orderCode);
        Task UpdateTransactionAsync(PaymentTransaction entity);
        Task<int> CountSuccessTransactionsAsync();
        Task<int> CountCancelTransactionsAsync();
    }
}