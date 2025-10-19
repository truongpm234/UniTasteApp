using System.Threading.Tasks;
using PaymentService.API.Models.Entity;

public interface IPaymentRepository
{
    Task<PaymentTransaction> AddTransactionAsync(PaymentTransaction entity);
    Task<Purchase> AddPurchaseAsync(Purchase p);
    Task<IEnumerable<PaymentTransaction>> GetAllTransactionsAsync();

}
