using RestaurantService.API.Models.Entity;

namespace RestaurantService.API.Repository
{
    public interface IUserReviewRepository
    {
        Task<List<Review>> GetReviewsByRestaurantIdAsync(int restaurantId);
        Task<Review> AddReviewAsync(Review review);
    }
}