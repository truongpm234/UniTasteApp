using SocialService.API.Models.DTO;
using SocialService.API.Service;
using System.Net.Http.Headers;

public class RestaurantApiService : IRestaurantApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RestaurantApiService(HttpClient httpClient, IConfiguration config, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _config = config;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string?> GetGooglePlaceIdAsync(int restaurantId)
    {
        var baseUrl = _config["ServiceUrls:ApiGateway"];
        var url = $"{baseUrl}/api/restaurants/get-all-restaurant-by-id?id={restaurantId}";
        Console.WriteLine($"[DEBUG] Calling URL: {url}");

        try
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
                Console.WriteLine($"[DEBUG] Attached token to RestaurantService request.");
            }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ERROR] RestaurantService returned: {response.StatusCode}");
                throw new Exception($"Không thể lấy dữ liệu từ RestaurantService. Code: {response.StatusCode}");
            }

            var restaurant = await response.Content.ReadFromJsonAsync<RestaurantResponseDto>();
            return restaurant?.GooglePlaceId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetGooglePlaceIdAsync: {ex.Message}");
            return null;
        }
    }
}
