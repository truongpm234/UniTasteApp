using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.API.Models.DTO;
using PaymentService.API.Models.Entity;
using PaymentService.API.Service;
using System.Security.Claims;

namespace PaymentService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPayOSService _payOSService;
        private readonly IPaymentService _paymentService;
        private readonly IPurchaseService _purchaseService;
        private readonly IServicePackageService _servicePackageService;

        public PaymentsController(IPayOSService payOSService, IPaymentService paymentService, IPurchaseService purchaseService, IServicePackageService servicePackageService)
        {
            _payOSService = payOSService;
            _paymentService = paymentService;
            _purchaseService = purchaseService;
            _servicePackageService = servicePackageService;
        }

        [Authorize]
        [HttpPost("manual-create-payment")]
        public async Task<IActionResult> ManualCreatePayment([FromBody] ManualCreatePaymentDto dto)
        {
            // 1. Lấy userId từ JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("UserId not found in token");
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized("UserId invalid");

            // 2. Lấy gói dịch vụ
            var package = (await _servicePackageService.GetAllServicePackagesAsync())
                .FirstOrDefault(p => p.ServicePackageId == dto.ServicePackageId);

            if (package == null)
                return BadRequest("Gói dịch vụ không tồn tại");

            // 3. Tạo bản ghi transaction thành công
            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var description = $"[Manual] Thanh toán {package.Name} ({package.DurationInMonths} tháng) - {dto.Note}";

            var paymentTransaction = new PaymentTransaction
            {
                UserId = userId,
                Amount = dto.Amount,
                TransactionType = "Manual", // Đánh dấu là tạo thủ công
                Status = "Success",         // Luôn thành công (manual)
                ReferenceId = Guid.NewGuid().ToString(),
                OrderCode = orderCode,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            await _paymentService.AddTransactionAsync(paymentTransaction);

            // 4. Tạo bản ghi Purchase
            var purchase = new Purchase
            {
                UserId = userId,
                PurchaseType = "Payment",
                Amount = dto.Amount,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                ServicePackageId = package.ServicePackageId
            };
            await _paymentService.AddPurchaseAsync(purchase);

            // 5. Trả kết quả
            return Ok(new
            {
                message = "Manual payment created successfully.",
                transactionId = paymentTransaction.PaymentTransactionId,
                purchaseId = purchase.PurchaseId,
                amount = dto.Amount,
                package = new
                {
                    package.ServicePackageId,
                    package.Name,
                    package.DurationInMonths
                }
            });
        }


        [HttpGet("success")]
        public IActionResult Success()
        {
            return Content("Payment Successful!");
        }

        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            return Content("Payment Cancelled!");
        }

        [Authorize]
        [HttpGet("get-all-payment-transaction")]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _paymentService.GetAllTransactionsAsync();
            return Ok(payments);
        }

        [HttpGet("payment-success-callback")]
        public async Task<IActionResult> PaymentSuccessCallback([FromQuery] long orderCode)
        {
            // 1. Tìm transaction theo orderCode
            var transaction = await _paymentService.GetTransactionByOrderCodeAsync(orderCode);
            if (transaction == null)
                return NotFound("Transaction not found");

            // 2. Update trạng thái transaction và purchase
            transaction.Status = "Active";
            await _paymentService.UpdateTransactionAsync(transaction);

            return Ok("Thanh toán thành công, gói đã được kích hoạt!");
        }

        [Authorize]
        [HttpGet("payment-cancel-callback")]
        public async Task<IActionResult> PaymentCancelCallback([FromQuery] long orderCode)
        {
            // 1. Tìm transaction theo orderCode
            var transaction = await _paymentService.GetTransactionByOrderCodeAsync(orderCode);
            if (transaction == null)
                return NotFound("Transaction not found");

            // 2. Update trạng thái transaction và purchase
            transaction.Status = "Cancel";
            await _paymentService.UpdateTransactionAsync(transaction);

            return Ok("Đã hủy gói thanh toán!");
        }

        [Authorize]
        [HttpGet("count-success-transactions")]
        public async Task<IActionResult> CountSuccessTransactions()
        {
            int count = await _paymentService.CountSuccessTransactionsAsync();
            return Ok(new { successTransactionCount = count });
        }

        [Authorize]
        [HttpGet("count-cancel-transactions")]
        public async Task<IActionResult> CountCancelTransactions()
        {
            int cancel = await _paymentService.CountCancelTransactionsAsync();
            return Ok(new { cancelTransactionCount = cancel });
        }

        [Authorize]
        [HttpGet("count-pending-transactions")]
        public async Task<IActionResult> CountPendingTransactions()
        {
            int pending = await _paymentService.CountPendingTransactionsAsync();
            return Ok(new { pendingTransactionCount = pending });
        }

        [Authorize]
        [HttpGet("sum-amount-success-transactions")]
        public async Task<double> SumAmountSuccessTransactions()
        {
            double total = await _paymentService.TotalAmountOfSuccessfulTransactionsAsync();
            return total;
        }

        [Authorize]
        [HttpGet("count-amount-of-paymentTransaction")]
        public async Task<int> CountAmountOfPaymentTransaction()
        {
            int count = await _paymentService.CountAmountOfPaymentTransactionAsync();
            return count;
        }

    }
}
