namespace RestaurantService.API.Service
{
    public interface IOpenAIService
    {
        Task<string> SummarizeReviewsAsync(string restaurantName, IEnumerable<string> reviews);
        Task<string> RecommendRestaurantsAsync(string userHistory, string search, List<string> restaurantNames);


    }
}