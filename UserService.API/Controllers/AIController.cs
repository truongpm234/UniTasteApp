using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using UserService.API.Models.DTO;
using UserService.API.Models.Entity;
using UserService.API.Repository;
using UserService.API.Service;
using UserService.API.Services;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IGeminiAIService _geminiAIService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        public AIController(IGeminiAIService geminiAIService, IHttpClientFactory httpClientFactory, IUserService userService, IConfiguration config)
        {
            _geminiAIService = geminiAIService;
            _httpClientFactory = httpClientFactory;
            _userService = userService;
            _config = config;
        }
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromQuery] string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                return BadRequest("Prompt is required.");
            }
            try
            {
                var response = await _geminiAIService.getChatResponse(prompt);
                return Ok(new ChatResponse { Response = response });
            }
            catch (Exception)
            {

                throw;
            }

        }

        [Authorize]
        [HttpPost("smart-recommend")]
        public async Task<IActionResult> SmartRecommend(
    [FromQuery] int userId,
    [FromQuery] string prompt,
    [FromQuery] double lat,
    [FromQuery] double lng)
        {
            var client = _httpClientFactory.CreateClient();

            // ✅ LẤY TOKEN
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            // ✅ DEBUG LOG
            Console.WriteLine($"[DEBUG] Token received: {token?.Substring(0, 20)}...");

            if (string.IsNullOrEmpty(token))
                return Unauthorized("No Authorization header!");

            // ✅ FORWARD TOKEN
            client.DefaultRequestHeaders.Add("Authorization", token);

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() ?? "production";
            var gatewayBase = Environment.GetEnvironmentVariable("GATEWAY_BASEURL");

            if (string.IsNullOrEmpty(gatewayBase))
            {
                if (env == "development")
                    gatewayBase = "http://localhost:8001";
                else
                    gatewayBase = "https://apigateway-otsb.onrender.com";
            }

            // 1. Lấy preference user
            string userPrefJson;
            var userPrefRes = await client.GetAsync($"{gatewayBase}/api/users/get-user-preference-by-userid/{userId}");
            if (!userPrefRes.IsSuccessStatusCode)
            {
                Console.WriteLine($"[AI] ❌ Không lấy được preference (Status: {userPrefRes.StatusCode})");

                // Retry nhẹ nếu 429
                if (userPrefRes.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(1000);
                    userPrefRes = await client.GetAsync($"{gatewayBase}/api/users/get-user-preference-by-userid/{userId}");
                }

                if (!userPrefRes.IsSuccessStatusCode)
                {
                    Console.WriteLine("[AI] ⚠️ Dùng preference rỗng để tiếp tục.");
                    userPrefJson = "{}";
                }
                else
                {
                    userPrefJson = await userPrefRes.Content.ReadAsStringAsync();
                }
            }
            else
            {
                userPrefJson = await userPrefRes.Content.ReadAsStringAsync();
            }

            // ==================================================================================
            // 2️⃣ Lấy danh sách quán gần nhất
            // ==================================================================================
            List<RestaurantDto>? restaurants = null;
            var resRes = await client.GetAsync($"{gatewayBase}/api/restaurants/get-nearest-restaurants?lat={lat}&lng={lng}&limit=10");

            if (!resRes.IsSuccessStatusCode)
            {
                Console.WriteLine($"[AI] ❌ Không lấy được danh sách quán (Status: {resRes.StatusCode})");

                if (resRes.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(1000);
                    resRes = await client.GetAsync($"{gatewayBase}/api/restaurants/get-nearest-restaurants?lat={lat}&lng={lng}&limit=10");
                }

                if (!resRes.IsSuccessStatusCode)
                {
                    Console.WriteLine("[AI] ⚠️ Gateway quá tải — dùng danh sách quán rỗng.");
                    restaurants = new List<RestaurantDto>();
                }
                else
                {
                    var restaurantJson = await resRes.Content.ReadAsStringAsync();
                    restaurants = System.Text.Json.JsonSerializer.Deserialize<List<RestaurantDto>>(restaurantJson);
                }
            }
            else
            {
                var restaurantJson = await resRes.Content.ReadAsStringAsync();
                restaurants = System.Text.Json.JsonSerializer.Deserialize<List<RestaurantDto>>(restaurantJson);
            }

            if (restaurants == null || restaurants.Count == 0)
            {
                Console.WriteLine("[AI] ⚠️ Không tìm thấy quán phù hợp gần người dùng.");
                restaurants = new List<RestaurantDto>();
            }

            List<ReviewGroupDto>? reviewDict = new();

            if (restaurants.Any())
            {
                var restIds = restaurants.Select(r => r.RestaurantId).ToList();
                var reviewContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(restIds),
                    Encoding.UTF8,
                    "application/json"
                );

                var reviewRes = await client.PostAsync(
                    $"{gatewayBase}/api/reviews/google/get-top-reviews-multiple?top=4",
                    reviewContent
                );

                if (!reviewRes.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[AI] ❌ Không lấy được review! (Status: {reviewRes.StatusCode})");

                    if (reviewRes.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(1000);
                        reviewRes = await client.PostAsync(
                            $"{gatewayBase}/api/reviews/google/get-top-reviews-multiple?top=4",
                            reviewContent
                        );
                    }

                    if (!reviewRes.IsSuccessStatusCode)
                    {
                        Console.WriteLine("[AI] ⚠️ Dùng review rỗng để tiếp tục.");
                        reviewDict = new List<ReviewGroupDto>();
                    }
                    else
                    {
                        var reviewJson = await reviewRes.Content.ReadAsStringAsync();
                        reviewDict = System.Text.Json.JsonSerializer.Deserialize<List<ReviewGroupDto>>(reviewJson)
                                     ?? new List<ReviewGroupDto>();
                    }
                }
                else
                {
                    var reviewJson = await reviewRes.Content.ReadAsStringAsync();
                    reviewDict = System.Text.Json.JsonSerializer.Deserialize<List<ReviewGroupDto>>(reviewJson)
                                 ?? new List<ReviewGroupDto>();
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("[USER REQUEST]: " + prompt);
            sb.AppendLine($"[USER LOCATION]: Latitude={lat}, Longitude={lng}");
            sb.AppendLine("[USER PREFERENCE]: " + userPrefJson);
            sb.AppendLine("[NEAREST RESTAURANTS]:");

            foreach (var rest in restaurants)
            {
                sb.AppendLine($"- {rest.Name} ({rest.GoogleRating ?? 0:F1}★):");
                sb.AppendLine($"    • Google Maps: https://www.google.com/maps/place/?q=place_id:{rest.GooglePlaceId}");
                if(rest.GooglePlaceId != null)
                {
                    sb.AppendLine($"    • Address: https://maps.google.com/?cid={rest.GooglePlaceId}");
                    Console.WriteLine($"[AI] Added address link for {rest.Name}");
                }

                var reviews = reviewDict?.FirstOrDefault(x => x.RestaurantId == rest.RestaurantId)?.Reviews;
                if (reviews != null)
                {
                    foreach (var rv in reviews.Take(4))
                        sb.AppendLine($"    • \"{rv.Comment}\" - {rv.UserName} ({rv.Rating}★)");
                }
            }

            sb.AppendLine("Trước tiên, nếu user hỏi câu không liên quan đến quán ăn thì hãy trả lời đúng trọng tâm. " +
                          "Nếu liên quan đến ăn uống, hãy linh hoạt gợi ý dựa vào vị trí gần (≤ 5km), " +
                          "không cần quá chú trọng preference nếu trống. " +
                          "Không được yêu cầu user mở Google Maps, không gọi API ngoài. " +
                          "Phải trả về ít nhất 5 quán ăn hoặc cafe phổ biến, giá hợp lý cho sinh viên, " +
                          "có đủ thông tin: tên, địa chỉ, giờ mở cửa, món nổi bật.");

            var aiResponse = await _geminiAIService.getChatResponse(sb.ToString());
            return Ok(new { answer = aiResponse });
        }
    }
}