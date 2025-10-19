using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace PaymentService.API.Service
{
    public interface IPayOSService
    {
        Task<string> CreatePaymentLink(long orderCode, long amount, string description);
    }
}