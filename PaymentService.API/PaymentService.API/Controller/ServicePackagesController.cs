using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.API.Models.DTO;
using PaymentService.API.Models.Entity;
using PaymentService.API.Service;
using System.Security.Claims;

namespace PaymentService.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicePackagesController : ControllerBase
    {
        private readonly IServicePackageService _servicePackageService;
        private readonly IPayOSService _payOSService;
        private readonly IPaymentService _paymentService;
        private readonly IPurchaseService _purchaseService;
        public ServicePackagesController(IServicePackageService servicePackageService, IPaymentService paymentService, IPayOSService payOSService, IPurchaseService purchaseService)
        {
            _servicePackageService = servicePackageService;
            _paymentService = paymentService;
            _payOSService = payOSService;
            _purchaseService = purchaseService;
        }

        [Authorize]
        [HttpGet("get-all-service-package")]
        public async Task<IActionResult> GetAllPackages()
        {
            var list = await _servicePackageService.GetAllServicePackagesAsync();
            return Ok(list);
        }

        [Authorize]
        [HttpPost("create-service-package-payment")]
        public async Task<IActionResult> CreateServicePackagePayment([FromBody] CreateServicePackagePaymentDto dto)
        {
            // Lấy userId từ JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("UserId not found in token");
            int userId = int.Parse(userIdClaim);

            // Lấy thông tin gói qua service
            var allPackages = await _servicePackageService.GetAllServicePackagesAsync();
            var packageEntity = allPackages.FirstOrDefault(p => p.ServicePackageId == dto.ServicePackageId);

            if (packageEntity == null)
                return BadRequest("Gói dịch vụ không tồn tại");

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var description = $"Thanh toán {packageEntity.Name} ({packageEntity.DurationInMonths} tháng)";
            var link = await _payOSService.CreatePaymentLink(orderCode, (long)packageEntity.Price, description);
            Console.WriteLine($"[DEBUG] orderCode={orderCode}, amount={packageEntity.Price}, description={description}");

            // Lưu giao dịch
            var transaction = new PaymentTransaction
            {
                UserId = userId,
                Amount = packageEntity.Price,
                TransactionType = "PayOS",
                Status = "Pending",
                ReferenceId = Guid.NewGuid().ToString(),
                OrderCode = orderCode,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            await _paymentService.AddTransactionAsync(transaction);

            // Lưu purchase
            var purchase = new Purchase
            {
                UserId = userId,
                PurchaseType = "ServicePackage",
                Amount = packageEntity.Price,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                ServicePackageId = packageEntity.ServicePackageId
            };
            await _paymentService.AddPurchaseAsync(purchase);

            return Ok(new
            {
                checkoutUrl = link,
                package = new
                {
                    packageEntity.ServicePackageId,
                    packageEntity.Name,
                    packageEntity.DurationInMonths,
                    packageEntity.Price,
                    packageEntity.Description
                }
            });
        }

        [Authorize]
        [HttpGet("check-service-package-status")]
        public async Task<IActionResult> CheckServicePackageStatus([FromQuery] int userId)
        {
            // Lấy userId từ JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("UserId not found in token");
            int userIdParse = int.Parse(userIdClaim);
            var hasPurchased = await _purchaseService.IsStatus(userIdParse);
            return Ok(new { hasPurchased });
        }

        [Authorize]
        [HttpGet("get-purchases-by-user-token")]
        public async Task<IActionResult> GetPurchasesByUser()
        {
            // Lấy userId từ JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("UserId not found in token");
            int userId = int.Parse(userIdClaim);

            var byUser = await _purchaseService.GetPurchasesByUserIdAsync(userId);
            return Ok(byUser);
        }

    }
}
