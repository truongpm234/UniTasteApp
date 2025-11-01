using Microsoft.AspNetCore.Mvc;
using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.GooglePlaces;
using RestaurantService.API.Service;

namespace RestaurantService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GooglePlacesController : ControllerBase
    {
        private readonly IGooglePlacesService _googlePlacesService;
        private readonly IRestaurantService _restaurantService;

        public GooglePlacesController(IGooglePlacesService googlePlacesService, IRestaurantService restaurantService)
        {
            _googlePlacesService = googlePlacesService;
            _restaurantService = restaurantService;
        }


        [HttpGet("search-nearby")]
        public async Task<IActionResult> SearchNearby([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] int radius = 5000, [FromQuery] string type = "restaurant", [FromQuery] string keyword = "")
        {
            var request = new GooglePlacesSearchRequest
            {
                Latitude = latitude,
                Longitude = longitude,
                Radius = radius,
                Type = type,
                Keyword = keyword
            };
            var results = await _googlePlacesService.SearchNearbyRestaurantsAsync(request);
            foreach (var p in results)
            {
                if (string.IsNullOrEmpty(p.FormattedAddress) && !string.IsNullOrEmpty(p.Vicinity))
                    p.FormattedAddress = p.Vicinity;
            }
            return Ok(results);
        }


        [HttpPost("search-import-nearby-full-data")]
        public async Task<IActionResult> SearchImportNearby([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] int radius = 10000, [FromQuery] string type = "restaurant", [FromQuery] string keyword = "")
        {
            var request = new GooglePlacesSearchRequest
            {
                Latitude = latitude,
                Longitude = longitude,
                Radius = radius,
                Type = type,
                Keyword = keyword
            };
            var imported = await _googlePlacesService.SearchAndImportNearbyAsync(request);
            var syncCount = await _googlePlacesService.SyncAllRestaurantCoverImagesAsync();

            return Ok(new { imported });
        }

        [HttpPost("search-import-nearby-with-paging")]
        public async Task<IActionResult> SearchImportNearby([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] int radius = 5000, [FromQuery] string type = "restaurant, cafe", [FromQuery] string keyword = "", [FromQuery] int currentPage = 1, [FromQuery] int pageSize = 5)
        {
            var request = new GooglePlacesSearchRequest
            {
                Latitude = latitude,
                Longitude = longitude,
                Radius = radius,
                Type = type,
                Keyword = keyword
            };

            var allPlaces = await _googlePlacesService.SearchNearbyRestaurantsAsync(request);
            var pagedPlaces = allPlaces.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            var pagedDtos = new List<GooglePlaceDTO>();
            foreach (var place in pagedPlaces)
            {
                int? priceRangeId = null;
                List<int> categoryIds = new List<int>();

                var restaurant = await _restaurantService.GetByGooglePlaceIdAsync(place.PlaceId);
                if (restaurant != null)
                {
                    priceRangeId = restaurant.PriceRangeId;
                    var categories = await _restaurantService.GetCategoriesByRestaurantIdAsync(restaurant.RestaurantId);
                    categoryIds = categories.Select(x => x.CategoryId).ToList();
                }
                else
                {
                    if (place.Types != null)
                    {
                        foreach (var typeName in place.Types)
                        {
                            var cat = await _restaurantService.GetOrCreateCategoryByNameAsync(typeName, "Google");
                            if (!categoryIds.Contains(cat.CategoryId))
                                categoryIds.Add(cat.CategoryId);
                        }
                    }
                    priceRangeId = place.PriceLevel != null
                        ? await _restaurantService.GetOrCreatePriceRangeIdAsync(place.PriceLevel)
                        : null;
                }

                var dto = new GooglePlaceDTO
                {
                    PlaceId = place.PlaceId,
                    Name = place.Name ?? "Chưa cập nhật",
                    FormattedAddress = place.FormattedAddress ?? place.Vicinity ?? "Chưa cập nhật",
                    Latitude = place.Geometry?.Location?.Lat ?? 0,
                    Longitude = place.Geometry?.Location?.Lng ?? 0,
                    Website = place.Website ?? "Chưa cập nhật",
                    Phone = place.FormattedPhoneNumber ?? "Chưa cập nhật",
                    PriceLevel = place.PriceLevel?.ToString() ?? "Chưa cập nhật",
                    Rating = place.Rating ?? 0,
                    OpeningHours = (place.OpeningHours?.WeekdayText?.Count > 0)
                        ? string.Join("; ", place.OpeningHours.WeekdayText)
                        : "Chưa cập nhật",
                    Types = place.Types ?? new List<string>(),
                    CoverImageUrl = (place.Photos != null && place.Photos.Any())
                        ? await _googlePlacesService.GetPhotoUrlAsync(place.Photos.First().PhotoReference, 800)
                        : null,
                    PriceRangeId = priceRangeId,
                    CategoryIds = categoryIds
                };
                pagedDtos.Add(dto);
            }

            var response = new
            {
                TotalItems = allPlaces.Count,
                TotalPages = (int)Math.Ceiling((double)allPlaces.Count / pageSize),
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = pagedDtos
            };

            _ = Task.Run(async () =>
            {
                using (var scope = HttpContext.RequestServices.CreateScope())
                {
                    var googlePlacesService = scope.ServiceProvider.GetRequiredService<IGooglePlacesService>();
                    await googlePlacesService.ImportDataAsync(request);
                }
            });

            return Ok(response);
        }
    }
}
