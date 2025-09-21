using Microsoft.AspNetCore.Mvc;
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

    // 1. Tìm kiếm trong bán kính 5km, kèm keyword tên (hoặc null)
    //[HttpGet("search")]
    //public async Task<ActionResult<List<Restaurant>>> Search(
    //    [FromQuery] double latitude,
    //    [FromQuery] double longitude,
    //    [FromQuery] string keyword = null,
    //    [FromQuery] double radiusKm = 5)
    //{
    //    var restaurants = await _restaurantService.SearchRestaurantsAsync(latitude, longitude, keyword, radiusKm);
    //    // Loại bỏ field null trước khi trả về (nếu muốn, dùng DTO để map kỹ hơn)
    //    foreach (var r in restaurants)
    //    {
    //        r.Name = r.Name ?? "";
    //        r.Address = r.Address ?? "";
    //        r.GooglePlaceId = r.GooglePlaceId ?? "";
    //        r.Phone = r.Phone ?? "";
    //        r.Website = r.Website ?? "";
    //        r.CoverImageUrl = r.CoverImageUrl ?? "";
    //        r.Status = r.Status ?? "";
    //        // ... tương tự các trường khác nếu cần
    //    }
    //    return Ok(restaurants);
    //}

    //// 2. Tìm kiếm quán ăn nước uống (lọc theo tên, không giới hạn vị trí)
    //[HttpGet("by-name")]
    //public async Task<ActionResult<List<Restaurant>>> SearchByName([FromQuery] string name)
    //{
    //    var restaurants = await _restaurantService.SearchRestaurantsAsync(0, 0, name, 99999); // radius cực lớn
    //    return Ok(restaurants);
    //}

    //// 3. Chi tiết quán ăn
    //[HttpGet("{id}")]
    //public async Task<ActionResult<Restaurant>> GetDetail(int id)
    //{
    //    var restaurant = await _restaurantService.GetRestaurantDetailAsync(id);
    //    if (restaurant == null)
    //        return NotFound();

    //    // Loại bỏ field null
    //    restaurant.Name = restaurant.Name ?? "";
    //    restaurant.Address = restaurant.Address ?? "";
    //    restaurant.GooglePlaceId = restaurant.GooglePlaceId ?? "";
    //    restaurant.Phone = restaurant.Phone ?? "";
    //    restaurant.Website = restaurant.Website ?? "";
    //    restaurant.CoverImageUrl = restaurant.CoverImageUrl ?? "";
    //    restaurant.Status = restaurant.Status ?? "";
    //    // ...

    //    return Ok(restaurant);
    //}
}
