using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
//using RestaurantService.API.Models.Entity;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantsController(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    [Authorize]
    [HttpGet]
    [Route("get-all-restaurant")]
    public async Task<IActionResult> GetAllRestaurant()
    {
        var restaurants = await _restaurantService.GetAllRestaurantAsync();
        int count = restaurants?.Count ?? 0;

        if (restaurants == null || restaurants.Count == 0)
        {
            return NotFound("No restaurants found.");
        }
        return Ok(new
        {
            count,
            restaurants
        });
    }

    [Authorize]
    [HttpPost("find-by-location-10km")]
    public async Task<IActionResult> GetRestaurantsByLocation([FromBody] LocationRequestDto location)
    {
        var restaurants = await _restaurantService.GetRestaurantsWithinRadiusAsync(location.Latitude, location.Longitude, location.RadiusKm);

        int count = restaurants?.Count ?? 0;

        if (count == 0)
        {
            return NotFound(new { count = 0, message = "No restaurants found within the specified radius." });
        }

        // Có thể trả về object gồm số lượng và danh sách
        return Ok(new
        {
            count,
            restaurants
        });
    }

    [Authorize]
    [HttpGet]
    [Route("get-all-restaurant-by-id")]
    public async Task<IActionResult> GetRestaurantByIdAsync(int id)
    {
        var restaurants = await _restaurantService.GetRestaurantByIdAsync(id);
        if (restaurants == null)
        {
            return NotFound("No restaurants found.");
        }
        return Ok(restaurants);
    }
}
