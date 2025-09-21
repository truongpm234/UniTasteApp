using Microsoft.EntityFrameworkCore;
using RestaurantService.API.Data.DBContext;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Models.GooglePlaces;

namespace RestaurantService.API.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly Exe201RestaurantServiceDbContext _context;
        public ReviewRepository(Exe201RestaurantServiceDbContext context) => _context = context;

        public async Task AddOrUpdateReviewsAsync(int restaurantId, List<GoogleReview> googleReviews)
        {
            var dbReviews = await _context.Reviews.Where(r => r.RestaurantId == restaurantId).ToListAsync();
            foreach (var gr in googleReviews)
            {
                var exist = dbReviews.FirstOrDefault(x => x.UserName == gr.AuthorName && x.Comment == gr.Text);
                if (exist == null)
                {
                    _context.Reviews.Add(new Review
                    {
                        RestaurantId = restaurantId,
                        UserName = gr.AuthorName,
                        Comment = gr.Text,
                        Rating = gr.Rating,
                        CreatedAt = DateTimeOffset.FromUnixTimeSeconds(gr.Time).UtcDateTime
                    });
                }
                // Có thể update nếu cần
            }
            await _context.SaveChangesAsync();
        }
    }
}
