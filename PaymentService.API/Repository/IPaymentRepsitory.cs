using System.Threading.Tasks;
using PaymentService.API.Models.Entity;

public interface IPaymentRepsitory
{
    Task<PaymentTransaction> AddTransactionAsync(PaymentTransaction entity);
    Task<Purchase> AddPurchaseAsync(Purchase p);
    Task<IEnumerable<PaymentTransaction>> GetAllTransactionsAsync();

}
