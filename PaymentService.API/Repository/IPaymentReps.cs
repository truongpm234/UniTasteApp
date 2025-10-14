using System.Threading.Tasks;
using PaymentService.API.Models.Entity;

public interface IPaymentReps
{
    Task AddTransactionAsync(PaymentTransaction entity);
    Task AddPurchaseAsync(Purchase p);
<<<<<<< HEAD
    Task<List<PaymentTransaction>> GetAllTransactionsAsync();

=======
>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0
}
