using RestaurantService.API.Models.GooglePlaces;

namespace RestaurantService.API.Repository
{
    public interface IReviewRepository
    {
        
            Task AddOrUpdateReviewsAsync(int restaurantId, List<GoogleReview> googleReviews);

        
    }
}