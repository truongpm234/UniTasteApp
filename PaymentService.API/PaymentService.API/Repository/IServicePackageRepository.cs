using Microsoft.EntityFrameworkCore;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Repository
{
    public interface IServicePackageRepository
    {
        Task<List<ServicePackage>> GetAllServicePackagesAsync();
        Task<int> GetServicePackageByIdAsync(int packageId);
        
    }
}