using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;

public interface IRestaurantService
{
    Task<List<Restaurant>> GetAllRestaurantAsync();
    Task<Restaurant> GetByGooglePlaceIdAsync(string googlePlaceId);
    Task<bool> ExistsByGooglePlaceIdAsync(string googlePlaceId);
    Task<Restaurant> CreateAsync(Restaurant restaurant);
    Task<Restaurant> UpdateAsync(Restaurant restaurant);
    Task<int?> GetOrCreatePriceRangeIdAsync(int? priceLevel);
    Task<Category?> GetCategoryByIdAsync(int categoryId);
    Task<int?> GetCategoryIdAsync(string name, string? sourceType = null);
    Task<int> GetOrCreateFeatureIdAsync(string name);
    Task<Category> GetOrCreateCategoryByNameAsync(string name, string sourceType = "Google");

}
