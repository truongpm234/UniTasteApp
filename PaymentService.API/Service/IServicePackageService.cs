using PaymentService.API.Models.Entity;

namespace PaymentService.API.Service
{
    public interface IServicePackageService
    {
        Task<List<ServicePackage>> GetAllServicePackagesAsync();
        Task<int> GetServicePackageByIdAsync(int packageId);
    }
}