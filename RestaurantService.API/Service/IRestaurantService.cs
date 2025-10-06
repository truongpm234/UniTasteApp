using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Models.GooglePlaces;

public interface IRestaurantService
{
    Task<List<Restaurant>> GetAllRestaurantAsync();
    Task<List<Restaurant>> GetRestaurantsWithinRadiusAsync(double latitude, double longitude, double radiusKm);
    Task<Restaurant> GetRestaurantByIdAsync(int id);
    Task<Restaurant> GetByGooglePlaceIdAsync(string googlePlaceId);
    Task<bool> ExistsByGooglePlaceIdAsync(string googlePlaceId);
    Task<Restaurant> CreateAsync(Restaurant restaurant);
    Task<Restaurant> UpdateAsync(Restaurant restaurant);
    Task<int?> GetOrCreatePriceRangeIdAsync(int? priceLevel);
    Task<Category?> GetCategoryByIdAsync(int categoryId);
    Task<int?> GetCategoryIdAsync(string name, string? sourceType = null);
    Task<int> GetOrCreateFeatureIdAsync(string name);
    Task<Restaurant> MapGooglePlaceToRestaurantAsync(GooglePlace place);
    Task<Category> GetOrCreateCategoryByNameAsync(string name, string sourceType = "Google");
    Task<List<Restaurant>> SearchRestaurantsByNameAsync(string name);


}


