using Microsoft.EntityFrameworkCore;
using PaymentService.API.Models.Entity;
using System.Threading.Tasks;

public interface IPaymentRepository
{
    Task<PaymentTransaction> AddTransactionAsync(PaymentTransaction entity);
    Task<Purchase> AddPurchaseAsync(Purchase p);
    Task<IEnumerable<PaymentTransaction>> GetAllTransactionsAsync();
    Task<PaymentTransaction?> GetTransactionByOrderCodeAsync(long orderCode);

    Task<PaymentTransaction?> GetTransactionByReferenceIdAsync(string referenceId);

    Task UpdateTransactionAsync(PaymentTransaction entity);
    Task<int> CountSuccessTransactionsAsync();
    Task<int> CountCancelTransactionsAsync();
    Task<int> CountPendingTransactionsAsync();
    Task<double> TotalAmountOfSuccessfulTransactionsAsync();
    Task<int> CountAmountOfPaymentTransactionAsync();

}
