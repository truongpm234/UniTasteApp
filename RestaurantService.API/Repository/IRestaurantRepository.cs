using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Models.GooglePlaces;

namespace RestaurantService.API.Repository
{
    public interface IRestaurantRepository
    {
        Task<List<Restaurant>> GetAllRestaurantAsync();
<<<<<<< HEAD
        Task<List<Category>> GetCategoriesByRestaurantIdAsync(int restaurantId);
        List<OpeningHourDto2> ParseOpeningHours(string openingHours);
=======
>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0
        Task<List<Restaurant>> GetRestaurantsWithinRadiusAsync(double latitude, double longitude, double radiusKm);
        Task<Restaurant> GetRestaurantByIdAsync(int id);
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
        Task<PaginationResult<List<Restaurant>>> SearchWithPagingAsync(string name, int currentPage, int pageSize);
        Task<PaginationResult<List<Restaurant>>> SearchByNameAndCategoryWithPagingAsync(string name, string categoryName, int currentPage, int pageSize);
        Task<List<Restaurant>> GetRestaurantsWithinRadiusAndCategoryAsync(double latitude, double longitude, double radiusKm, string categoryName);
=======
>>>>>>> a1129b948ccc7d4674db9eb146672d75d5e673f0


    }

}