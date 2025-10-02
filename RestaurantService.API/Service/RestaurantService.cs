using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Models.GooglePlaces;
using RestaurantService.API.Repository;
using RestaurantService.API.Service;

namespace RestaurantService.API.Service
{

    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _restaurantRepo;

        public RestaurantService(IRestaurantRepository restaurantRepo)
        {
            _restaurantRepo = restaurantRepo;
        }
        public async Task<List<Restaurant>> GetAllRestaurantAsync()
        {
            return await _restaurantRepo.GetAllRestaurantAsync();
        }
        public async Task<Restaurant> GetByGooglePlaceIdAsync(string googlePlaceId)
        {
            return await _restaurantRepo.GetByGooglePlaceIdAsync(googlePlaceId);
        }
        
        public async Task<bool> ExistsByGooglePlaceIdAsync(string googlePlaceId)
        {
            return await _restaurantRepo.ExistsByGooglePlaceIdAsync(googlePlaceId);
        }
        public async Task<Restaurant> CreateAsync(Restaurant restaurant)
        {
            return await _restaurantRepo.CreateAsync(restaurant);
        }
        public async Task<Restaurant> UpdateAsync(Restaurant restaurant)
        {
            return await _restaurantRepo.UpdateAsync(restaurant);
        }
        public async Task<int?> GetOrCreatePriceRangeIdAsync(int? priceLevel)
        {
            return await _restaurantRepo.GetOrCreatePriceRangeIdAsync(priceLevel);
        }
        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _restaurantRepo.GetCategoryByIdAsync(categoryId);
        }

        public async Task<int?> GetCategoryIdAsync(string name, string? sourceType = null) 
        {
            return await _restaurantRepo.GetCategoryIdAsync(name, sourceType);
        }

        public async Task<int> GetOrCreateFeatureIdAsync(string name)
        {
            return await _restaurantRepo.GetOrCreateFeatureIdAsync(name);
        }
        public async Task<Category> GetOrCreateCategoryByNameAsync(string name, string sourceType = "Google")
        {
            return await _restaurantRepo.GetOrCreateCategoryByNameAsync(name, sourceType);
        }

        public async Task<Restaurant> GetRestaurantByIdAsync(int id)
        {
            return await _restaurantRepo.GetRestaurantByIdAsync(id);
        }

        public async Task<Restaurant> MapGooglePlaceToRestaurantAsync(GooglePlace place)
        {
            return await _restaurantRepo.MapGooglePlaceToRestaurantAsync(place);
        }
    }

}
