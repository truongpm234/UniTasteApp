using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Models.GooglePlaces;

public interface IRestaurantService
{
    Task<List<Restaurant>> GetAllRestaurantAsync();
<<<<<<< HEAD
    Task<List<RestaurantResponseDto>> GetAllRestaurantFullAsync();
    Task<List<Restaurant>> GetRestaurantsWithinRadiusAsync(double latitude, double longitude, double radiusKm);
    Task<RestaurantResponseDto?> GetRestaurantByIdAsync(int id);
    Task<List<Category>> GetCategoriesByRestaurantIdAsync(int restaurantId);
=======
    Task<List<Restaurant>> GetRestaurantsWithinRadiusAsync(double latitude, double longitude, double radiusKm);
    Task<Restaurant> GetRestaurantByIdAsync(int id);
>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0
    Task<Restaurant> GetByGooglePlaceIdAsync(string googlePlaceId);
    Task<bool> ExistsByGooglePlaceIdAsync(string googlePlaceId);
    Task<Restaurant> CreateAsync(Restaurant restaurant);
    Task<Restaurant> UpdateAsync(Restaurant restaurant);
    Task SyncCategoriesAsync(Restaurant restaurant, List<string> googleTypes);
    Task<int?> GetOrCreatePriceRangeIdAsync(int? priceLevel);
    Task<Category?> GetCategoryByIdAsync(int categoryId);
    Task<int?> GetCategoryIdAsync(string name, string? sourceType = null);
    Task<int> GetOrCreateFeatureIdAsync(string name);
    Task<Restaurant> MapGooglePlaceToRestaurantAsync(GooglePlace place);
    Task<Category> GetOrCreateCategoryByNameAsync(string name, string sourceType = "Google");
    Task<List<Restaurant>> SearchRestaurantsByNameAsync(string name);
<<<<<<< HEAD
    Task<PaginationResult<List<GetCategoryIdByRestaurantIdDto>>> SearchWithPagingAsync(string name, int currentPage, int pageSize);
    Task<PaginationResult<List<GetCategoryIdByRestaurantIdDto>>> SearchByNameAndCategoryWithPagingAsync(string name, string categoryName, int currentPage, int pageSize);
    Task<List<RestaurantResponseDto>> GetRestaurantsWithinRadiusAndCategoryAsync(double latitude, double longitude, double radiusKm, string categoryName);
=======

>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0

}


