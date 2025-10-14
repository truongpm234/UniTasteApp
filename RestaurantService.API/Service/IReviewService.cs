using RestaurantService.API.Models.Entity;
using RestaurantService.API.Models.GooglePlaces;

namespace RestaurantService.API.Service
{
    public interface IReviewService
    {
        Task AddOrUpdateReviewsAsync(int restaurantId, List<GoogleReview> googleReviews);
        Task<List<Review>> GetAllReviewsAsync();
        Task<List<Review>> GetTopReviewsByRestaurantIdAsync(int restaurantId, int top = 4);   //AI

    }
}
