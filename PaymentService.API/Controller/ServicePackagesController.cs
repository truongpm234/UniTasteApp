using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.API.Service;

namespace PaymentService.API.Controller
{
    public class ServicePackagesController : ControllerBase
    {
        private readonly IServicePackageService _servicePackageService;

        public ServicePackagesController(IServicePackageService servicePackageService)
        {
            _servicePackageService = servicePackageService;
        }

        [Authorize]
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllPackages()
        {
            var list = await _servicePackageService.GetAllServicePackagesAsync();
            return Ok(list);
        }


    }
}
