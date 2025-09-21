using RestaurantService.API.Models.GooglePlaces;

namespace RestaurantService.API.Service
{
    public interface IReviewService
    {
        
            Task AddOrUpdateReviewsAsync(int restaurantId, List<GoogleReview> googleReviews);
        
    }
}
