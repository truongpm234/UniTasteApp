using Microsoft.EntityFrameworkCore;
using RestaurantService.API.Data.DBContext;
using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Models.GooglePlaces;
using RestaurantService.API.Service;
using System;

namespace RestaurantService.API.Repository
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly Exe201RestaurantServiceDbContext _context;
        public RestaurantRepository(Exe201RestaurantServiceDbContext context)
        {
            _context = context;
        }
        public async Task<List<Restaurant>> GetAllRestaurantAsync()
        {
            return await _context.Restaurants
    .Include(r => r.Categories)
    .Include(r => r.Features)
    .Include(r => r.PriceRange)
    .Include(r => r.Reviews)
    .ToListAsync();

        }

        public async Task<Restaurant> GetRestaurantByIdAsync(int id)
        {
            return await _context.Restaurants
                .Include(r => r.Categories)
                .Include(r => r.Features)
                .Include(r => r.PriceRange)
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(r => r.RestaurantId == id);
        }
        public async Task<Restaurant> GetByGooglePlaceIdAsync(string googlePlaceId)
        {
            return await _context.Restaurants
                .Include(r => r.Categories)
                .Include(r => r.Features)
                .Include(r => r.PriceRange)
                .Include(r => r.Reviews)
                //.Include(r => r.OpeningHoursNavigation)
                .FirstOrDefaultAsync(r => r.GooglePlaceId == googlePlaceId);
        }

        public async Task<bool> ExistsByGooglePlaceIdAsync(string googlePlaceId)
        {
            return await _context.Restaurants.AnyAsync(r => r.GooglePlaceId == googlePlaceId);
        }

        public async Task<Restaurant> CreateAsync(Restaurant restaurant)
        {
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();
            return restaurant;
        }

        public async Task<Restaurant> UpdateAsync(Restaurant restaurant)
        {
            _context.Restaurants.Update(restaurant);
            await _context.SaveChangesAsync();
            return restaurant;
        }
        public async Task<Restaurant> MapGooglePlaceToRestaurantAsync(GooglePlace place)
        {
            if (place == null) throw new ArgumentNullException(nameof(place));

            string openingHoursStr = "Chưa cập nhật";
            if (place.OpeningHours?.WeekdayText?.Count > 0)
                openingHoursStr = string.Join("; ", place.OpeningHours.WeekdayText);

            int priceRangeId = (int)await GetOrCreatePriceRangeIdAsync(place.PriceLevel);

            return new Restaurant
            {
                Name = place?.Name ?? "Chưa cập nhật",
                Address = place?.Vicinity ?? "Chưa cập nhật",
                Latitude = place?.Geometry?.Location?.Lat ?? 0,
                Longitude = place?.Geometry?.Location?.Lng ?? 0,
                GooglePlaceId = place?.PlaceId ?? "",
                Phone = place?.FormattedPhoneNumber ?? "Chưa cập nhật",
                Website = place?.Website ?? "Chưa cập nhật",
                //CoverImageUrl = (place?.Photos?.FirstOrDefault() != null)
                //    ? await _googlePlacesService.GetPhotoUrlAsync(place?.Photos?.First().PhotoReference, 800)
                //    : "",
                GoogleRating = place?.Rating ?? 0,
                OpeningHours = openingHoursStr,
                PriceRangeId = priceRangeId,
                CreatedAt = DateTime.UtcNow,
                Status = "Active"
            };
        }

        public async Task<int?> GetOrCreatePriceRangeIdAsync(int? priceLevel)
        {
            if (priceLevel == null) return 1; // default: "Unknown" hoặc Free
            var name = priceLevel switch
            {
                0 => "Free",
                1 => "Inexpensive",
                2 => "Moderate",
                3 => "Expensive",
                4 => "Very Expensive",
                _ => "Unknown"
            };
            var range = await _context.PriceRanges.FirstOrDefaultAsync(x => x.Name == name);
            if (range != null) return range.PriceRangeId;

            var entity = new PriceRange { Name = name };
            _context.PriceRanges.Add(entity);
            await _context.SaveChangesAsync();
            return entity.PriceRangeId;
        }
        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories.FindAsync(categoryId);
        }

        public async Task<int?> GetCategoryIdAsync(string name, string? sourceType = null)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Name == name && x.SourceType == sourceType);
            return category?.CategoryId;
        }


        public async Task<int> GetOrCreateFeatureIdAsync(string name)
        {
            var feature = await _context.Features.FirstOrDefaultAsync(x => x.Name == name);
            if (feature != null) return feature.FeatureId;
            var entity = new Feature { Name = name };
            _context.Features.Add(entity);
            await _context.SaveChangesAsync();
            return entity.FeatureId;

        }
        public async Task<Category> GetOrCreateCategoryByNameAsync(string name, string sourceType = "Google")
        {
            name = name?.Trim() ?? "";
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Name == name && x.SourceType == sourceType);
            if (category != null)
                return category;

            // Nếu chưa có thì tạo mới
            category = new Category
            {
                Name = name,
                SourceType = sourceType
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

    }
}