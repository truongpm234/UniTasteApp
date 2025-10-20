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

        public PaymentsController(IPayOSService payOSService, IPaymentService paymentService, IPurchaseService purchaseService)
        {
            _payOSService = payOSService;
            _paymentService = paymentService;
            _purchaseService = purchaseService;
        }

        [Authorize]
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //create ordercode new/1time
            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // ✅ Lấy UserId từ JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("UserId not found in token");

            int userId = int.Parse(userIdClaim);

            // 1. Gọi PayOS
            var link = await _payOSService.CreatePaymentLink(dto.OrderCode, dto.Amount, dto.Description);

            // 2. Lưu transaction
            var transaction = new PaymentTransaction
            {
                UserId = userId,
                Amount = dto.Amount,
                TransactionType = "PayOS",
                Status = "Pending",
                ReferenceId = Guid.NewGuid().ToString(),
                OrderCode = dto.OrderCode,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentService.AddTransactionAsync(transaction);

            return Ok(new { checkoutUrl = link });
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
    }
}
