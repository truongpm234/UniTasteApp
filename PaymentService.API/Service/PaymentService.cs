using Microsoft.EntityFrameworkCore;
using PaymentService.API.Models.Entity;
using PaymentService.API.Repository;

namespace PaymentService.API.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IPaymentRepository repository, ILogger<PaymentService> logger)
        {
            _repository = repository;
            _logger = logger;
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

        public async Task MarkPaidAsync(string paymentLinkId, long amount)
        {
            var tx = await _repository.GetTransactionByReferenceIdAsync(paymentLinkId);
            if (tx is null)
            {
                _logger.LogWarning("MarkPaid: transaction not found by ReferenceId {Ref}", paymentLinkId);
                throw new InvalidOperationException("Payment transaction not found");
            }

            // Khớp với conventions hiện có của repo: "Success" | "Cancel" | "Pending"
            tx.Status = "Success";
            // Amount trong entity là decimal?
            if (amount > 0) tx.Amount = (decimal)amount;
            if (string.IsNullOrWhiteSpace(tx.ReferenceId)) tx.ReferenceId = paymentLinkId;

            await _repository.UpdateTransactionAsync(tx);
        }

        public async Task MarkCanceledAsync(string paymentLinkId)
        {
            var tx = await _repository.GetTransactionByReferenceIdAsync(paymentLinkId);
            if (tx is null)
            {
                _logger.LogWarning("MarkCanceled: transaction not found by ReferenceId {Ref}", paymentLinkId);
                return;
            }

            tx.Status = "Cancel";
            if (string.IsNullOrWhiteSpace(tx.ReferenceId)) tx.ReferenceId = paymentLinkId;

            await _repository.UpdateTransactionAsync(tx);
        }

        public async Task MarkFailedAsync(string paymentLinkId)
        {
            var tx = await _repository.GetTransactionByReferenceIdAsync(paymentLinkId);
            if (tx is null)
            {
                _logger.LogWarning("MarkFailed: transaction not found by ReferenceId {Ref}", paymentLinkId);
                return;
            }

            // Bạn có thể chọn "Pending" hoặc thêm hẳn "Failed".
            // Để khớp các hàm thống kê hiện có, mình tạm set "Pending".
            tx.Status = "Pending";
            if (string.IsNullOrWhiteSpace(tx.ReferenceId)) tx.ReferenceId = paymentLinkId;

            await _repository.UpdateTransactionAsync(tx);
        }
    }
}