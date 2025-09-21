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

        public GooglePlacesController(IGooglePlacesService googlePlacesService)
        {
            _googlePlacesService = googlePlacesService;
        }

        /// <summary>
        /// Search (Google API) các quán ăn/cafe gần vị trí chỉ định (realtime, KHÔNG lấy DB)
        /// </summary>
        [HttpGet("search-nearby")]
        public async Task<IActionResult> SearchNearby([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] int radius = 5000, [FromQuery] string type = "restaurant", [FromQuery] string keyword = "")
        {
            var request = new GooglePlacesSearchRequest
            {
                Latitude = latitude,
                Longitude = longitude,
                Radius = radius,
                Type = type,    //cafe, restaurant, food, bar, meal_takeaway, meal_delivery
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

        /// <summary>
        /// Import (lưu DB) tất cả quán ăn/cafe gần vị trí chỉ định từ Google Place API (chỉ lưu các quán chưa có GooglePlaceId trong DB)
        /// </summary>
        [HttpPost("search-import-nearby")]
        public async Task<IActionResult> SearchImportNearby([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] int radius = 5000, [FromQuery] string type = "restaurant", [FromQuery] string keyword = "")
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
    }
}
