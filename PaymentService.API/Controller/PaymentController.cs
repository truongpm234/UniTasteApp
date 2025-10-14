using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.API.Models.Entity;
using PaymentService.API.Service;
using System.Security.Claims;

namespace PaymentService.API.Controllers
{
    [Route("payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PayOSService _payOSService;
        private readonly IPaymentReps _paymentRepository;

        public PaymentController(PayOSService payOSService, IPaymentReps paymentRepository)
        {
            _payOSService = payOSService;
            _paymentRepository = paymentRepository;
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create(long orderCode, long amount, string description)
        {
            var link = await _payOSService.CreatePaymentLink(orderCode, amount, description);
            return Redirect(link);
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
                UserId = userId,  // ✅ lấy từ token, không lấy từ client
                Amount = dto.Amount,
                TransactionType = "Payment",
                Status = "Pending",
                ReferenceId = Guid.NewGuid().ToString(),
                OrderCode = dto.OrderCode,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddTransactionAsync(transaction);

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
<<<<<<< HEAD

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllTransactions()
        {
            var transactions = await _paymentRepository.GetAllTransactionsAsync();
            return Ok(transactions);
        }

    }
=======
    }

>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0
    // DTO cho request body
    public class CreatePaymentDto
    {
        public long OrderCode { get; set; }
        public long Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
