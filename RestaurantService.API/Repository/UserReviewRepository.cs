using Microsoft.EntityFrameworkCore;
using RestaurantService.API.Data.DBContext;
using RestaurantService.API.Models.Entity;

namespace RestaurantService.API.Repository
{
    public class UserReviewRepository : IUserReviewRepository
    {
        private readonly Exe201RestaurantServiceDbContext _context;
        public UserReviewRepository(Exe201RestaurantServiceDbContext context)
        {
            _context = context;
        }

        public async Task<List<Review>> GetReviewsByRestaurantIdAsync(int restaurantId)
        {
            return await _context.Reviews
                .Where(r => r.RestaurantId == restaurantId)
                .OrderByDescending(r => r.CreatedAt)
                .Include(r => r.PhotoReviews)
                .ToListAsync();
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<bool> ExistsReviewByUserAsync(int userId, int restaurantId)
        {
            return await _context.Reviews.AnyAsync(r => r.UserId == userId && r.RestaurantId == restaurantId);
        }

    }
}
