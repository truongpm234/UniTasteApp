using System.Threading.Tasks;
using PaymentService.API.Models.Entity;

public interface IPaymentRepsitory
{
    Task AddTransactionAsync(PaymentTransaction entity);
    Task AddPurchaseAsync(Purchase p);
    Task<IEnumerable<PaymentTransaction>> GetAllTransactionsAsync();

}
