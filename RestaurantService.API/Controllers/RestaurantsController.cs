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

    //[Authorize]
    [HttpGet]
    [Route("get-all-restaurant")]
    public async Task<IActionResult> GetAllRestaurant()
    {
        var restaurants = await _restaurantService.GetAllRestaurantFullAsync();
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

    //[Authorize]
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


    [HttpGet("search")]
    public async Task<IActionResult> SearchRestaurantByName([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Restaurant name is required.");

        var restaurants = await _restaurantService.SearchRestaurantsByNameAsync(name);
        int count = restaurants?.Count ?? 0;

        if (count == 0)
            return NotFound(new { count = 0, message = "No restaurants found matching the name." });

        return Ok(new
        {
            count,
            restaurants
        });
    }

    [Authorize]
    [HttpGet("search-by-name-with-paging")]
    public async Task<IActionResult> SearchWithPagingAsync([FromQuery] string name, [FromQuery] int currentPage = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Restaurant name is required.");
        if (currentPage <= 0) currentPage = 1;
        if (pageSize <= 0) pageSize = 10;

        var result = await _restaurantService.SearchWithPagingAsync(name, currentPage, pageSize);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("search-by-name-and-category")]
    public async Task<IActionResult> SearchByNameAndCategory([FromQuery] string name, [FromQuery] string category)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category))
            return BadRequest("Name and Category are required.");

        var restaurants = await _restaurantService.SearchByNameAndCategoryAsync(name, category);
        int count = restaurants?.Count ?? 0;

        if (count == 0)
            return Ok(new { count = 0, message = "No restaurants found matching the name and category." });

        return Ok(new
        {
            count,
            restaurants
        });
    }

    [Authorize]
    [HttpPost("find-by-location-and-category")]
    public async Task<IActionResult> GetRestaurantsWithinRadiusAndCategory([FromBody] LocationCategoryRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.CategoryName))
            return BadRequest("Invalid request. Please provide location and category name.");

        var restaurants = await _restaurantService.GetRestaurantsWithinRadiusAndCategoryAsync(request.Latitude, request.Longitude, request.RadiusKm, request.CategoryName);
        int count = restaurants?.Count ?? 0;

        return Ok(new
        {
            count,
            restaurants
        });
    }

}
