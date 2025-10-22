namespace SocialService.API.Service
{
    public interface IRestaurantApiService
    {
        Task<string?> GetGooglePlaceIdAsync(int restaurantId);
    }
}
