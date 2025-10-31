using Microsoft.EntityFrameworkCore;
using PaymentService.API.Data.DBContext;
using PaymentService.API.Models.Entity;

namespace PaymentService.API.Repository
{
    public class ServicePackageRepository : IServicePackageRepository
    {
        private readonly Exe201PaymentServiceDbContext _context;

        public ServicePackageRepository(Exe201PaymentServiceDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServicePackage>> GetAllServicePackagesAsync()
        {
            return await _context.ServicePackages.ToListAsync();
        }

        public async Task<int> GetServicePackageByIdAsync(int packageId)
        {
            var package = await _context.ServicePackages.FirstOrDefaultAsync(p => p.ServicePackageId == packageId);
            if (package == null)
            {
                throw new Exception("Service package not found");
            }
            return package.ServicePackageId;
        }

    }
}
