using Microsoft.EntityFrameworkCore;
using SocialService.API.Data.DBContext;
using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public class UserFeedbackRepository : IUserFeedbackRepository
    {
        private readonly Exe201SocialServiceDbContext _context;
        public UserFeedbackRepository(Exe201SocialServiceDbContext context) => _context = context;

        public async Task<UserFeedback> AddAsync(UserFeedback feedback)
        {
            _context.UserFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<List<UserFeedback>> GetAllAsync()
        {
            return await _context.UserFeedbacks.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync()
        {
            return await _context.UserFeedbacks.AnyAsync() ?
                await _context.UserFeedbacks.AverageAsync(x => x.Rating) : 0.0;
        }
    }

}
