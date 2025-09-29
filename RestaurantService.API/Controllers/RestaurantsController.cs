using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    [Route("get-all-restaurant")]
    public async Task<IActionResult> GetAllRestaurant()
    {
        var restaurants = await _restaurantService.GetAllRestaurantAsync();
        if (restaurants == null || restaurants.Count == 0)
        {
            return NotFound("No restaurants found.");
        }
        return Ok(restaurants);
    }
}
