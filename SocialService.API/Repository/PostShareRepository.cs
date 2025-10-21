using SocialService.API.Data.DBContext;
using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public class PostShareRepository : IPostShareRepository
    {
        private readonly Exe201SocialServiceDbContext _context;
        public PostShareRepository(Exe201SocialServiceDbContext context)
        {
            _context = context;
        }

        public async Task AddShareAsync(PostShare share)
        {
            await _context.PostShares.AddAsync(share);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
