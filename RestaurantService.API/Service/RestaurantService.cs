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

        public async Task<List<RestaurantResponseDto>> GetAllRestaurantFullAsync()
        {
            var restaurants = await _restaurantRepo.GetAllRestaurantAsync();
            var result = new List<RestaurantResponseDto>();

            foreach (var r in restaurants)
            {
                // ✅ Get categories for specific restaurant only
                var categories = await _restaurantRepo.GetCategoriesByRestaurantIdAsync(r.RestaurantId);

                // ✅ Parse opening hours to OpeningHourDto2
                var openingHours = _restaurantRepo.ParseOpeningHours(r.OpeningHours ?? string.Empty);

                result.Add(new RestaurantResponseDto
                {
                    RestaurantId = r.RestaurantId,
                    Name = r.Name,
                    Address = r.Address,
                    Latitude = r.Latitude ?? 0,
                    Longitude = r.Longitude ?? 0,
                    GooglePlaceId = r.GooglePlaceId,
                    Phone = r.Phone,
                    Website = r.Website,
                    CoverImageUrl = r.CoverImageUrl,
                    GoogleRating = r.GoogleRating,
                    PriceRangeId = r.PriceRangeId,
                    CreatedAt = r.CreatedAt,
                    Status = r.Status,
                    PriceRange = r.PriceRange,
                    Categories = categories,
                    Features = r.Features?.ToList() ?? new List<Feature>(),
                    Reviews = r.Reviews?.ToList() ?? new List<Review>(),
                    OpeningHours = openingHours
                });
            }

            return result;
        }
        public async Task<List<Restaurant>> GetRestaurantsWithinRadiusAsync(double latitude, double longitude, double radiusKm)
        {
            return await _restaurantRepo.GetRestaurantsWithinRadiusAsync(latitude, longitude, radiusKm);
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
        public async Task<List<Restaurant>> SearchRestaurantsByNameAsync(string name)
        {

            return await _restaurantRepo.SearchRestaurantsByNameAsync(name);
        }
        public async Task<List<RestaurantResponseDto>> SearchByNameAndCategoryAsync(string name, string categoryName)
        {
            var restaurants = await _restaurantRepo.SearchByNameAndCategoryAsync(name, categoryName);
            var result = new List<RestaurantResponseDto>();

            foreach (var r in restaurants)
            {
                var categories = await _restaurantRepo.GetCategoriesByRestaurantIdAsync(r.RestaurantId);

                var openingHourDto2List = _restaurantRepo.ParseOpeningHours(r.OpeningHours ?? string.Empty);

                result.Add(new RestaurantResponseDto
                {
                    RestaurantId = r.RestaurantId,
                    Name = r.Name,
                    Address = r.Address,
                    Latitude = (double)r.Latitude,
                    Longitude = (double)r.Longitude,
                    GooglePlaceId = r.GooglePlaceId,
                    Phone = r.Phone,
                    Website = r.Website,
                    CoverImageUrl = r.CoverImageUrl,
                    GoogleRating = r.GoogleRating,
                    PriceRangeId = r.PriceRangeId,
                    CreatedAt = r.CreatedAt,
                    Status = r.Status,
                    PriceRange = r.PriceRange,
                    Categories = categories.ToList(),
                    Features = r.Features?.ToList() ?? new List<Feature>(),
                    Reviews = r.Reviews?.ToList() ?? new List<Review>(),
                    OpeningHours = openingHourDto2List
                });
            }
            return result;
        }

        public async Task<List<RestaurantResponseDto>> GetRestaurantsWithinRadiusAndCategoryAsync(double latitude, double longitude, double radiusKm, string categoryName)
        {
            var restaurants = await _restaurantRepo.GetRestaurantsWithinRadiusAndCategoryAsync(latitude, longitude, radiusKm, categoryName);
            var result = new List<RestaurantResponseDto>();
            foreach (var r in restaurants)
            {
                var categories = await _restaurantRepo.GetCategoriesByRestaurantIdAsync(r.RestaurantId);
                var openingHourDto2List = _restaurantRepo.ParseOpeningHours(r.OpeningHours ?? string.Empty);
                result.Add(new RestaurantResponseDto
                {
                    RestaurantId = r.RestaurantId,
                    Name = r.Name,
                    Address = r.Address,
                    Latitude = (double)r.Latitude,
                    Longitude = (double)r.Longitude,
                    GooglePlaceId = r.GooglePlaceId,
                    Phone = r.Phone,
                    Website = r.Website,
                    CoverImageUrl = r.CoverImageUrl,
                    GoogleRating = r.GoogleRating,
                    PriceRangeId = r.PriceRangeId,
                    CreatedAt = r.CreatedAt,
                    Status = r.Status,
                    PriceRange = r.PriceRange,
                    Categories = categories.ToList(),
                    Features = r.Features?.ToList() ?? new List<Feature>(),
                    Reviews = r.Reviews?.ToList() ?? new List<Review>(),
                    OpeningHours = openingHourDto2List
                });
            }
            return result;
        }

    }

}
