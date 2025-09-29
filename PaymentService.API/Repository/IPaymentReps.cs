using System.Threading.Tasks;
using PaymentService.API.Models.Entity;

public interface IPaymentReps
{
    Task AddTransactionAsync(PaymentTransaction entity);
    Task AddPurchaseAsync(Purchase p);
}
