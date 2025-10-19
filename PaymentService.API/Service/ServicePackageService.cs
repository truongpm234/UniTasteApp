using PaymentService.API.Models.Entity;
using PaymentService.API.Repository;

namespace PaymentService.API.Service
{
    public class ServicePackageService : IServicePackageService
    {
        private readonly IServicePackageRepository _servicePackageRepository;
        public ServicePackageService(IServicePackageRepository servicePackageRepository)
        {
            _servicePackageRepository = servicePackageRepository;
        }
        public async Task<List<ServicePackage>> GetAllServicePackagesAsync()
        {
            return await _servicePackageRepository.GetAllServicePackagesAsync();
        }
        public async Task<int> GetServicePackageByIdAsync(int packageId)
        {
            return await _servicePackageRepository.GetServicePackageByIdAsync(packageId);
        }
    }
}
