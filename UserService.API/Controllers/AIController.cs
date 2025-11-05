using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        private readonly ILogger<AIController> _logger;

        public AIController(
            IGeminiAIService geminiAIService,
            IHttpClientFactory httpClientFactory,
            IUserService userService,
            ILogger<AIController> logger)
        {
            _geminiAIService = geminiAIService;
            _httpClientFactory = httpClientFactory;
            _userService = userService;
            _logger = logger;
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Chat endpoint");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("smart-recommend")]
        public async Task<IActionResult> SmartRecommend(
            [FromQuery] int userId,
            [FromQuery] string prompt,
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] int radius = 5000)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

                if (string.IsNullOrEmpty(token))
                    return Unauthorized("No Authorization header!");

                client.DefaultRequestHeaders.Add("Authorization", token);

                // Base URL
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() ?? "production";
                var gatewayBase = Environment.GetEnvironmentVariable("GATEWAY_BASEURL");
                if (string.IsNullOrEmpty(gatewayBase))
                    gatewayBase = env == "development" ? "http://localhost:8001" : "https://apigateway-otsb.onrender.com";

                _logger.LogInformation($"Gateway Base URL: {gatewayBase}");

                // 1. Lấy user preference
                string userPrefJson = "Không có sở thích đặc biệt";
                try
                {
                    var userPrefRes = await client.GetAsync($"{gatewayBase}/api/users/get-user-preference-by-userid/{userId}");
                    if (userPrefRes.IsSuccessStatusCode)
                    {
                        var content = await userPrefRes.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(content) && content != "{}")
                        {
                            userPrefJson = content;
                        }
                        _logger.LogInformation($"User preference fetched: {userPrefJson}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching user preference");
                }

                // 2. Lấy danh sách quán từ Google Places API
                List<GooglePlaceSimpleDto> restaurants = new();
                try
                {
                    var googlePlacesUrl = $"{gatewayBase}/api/GooglePlaces/search-nearby?latitude={lat}&longitude={lng}&radius={radius}&type=restaurant&keyword=";
                    _logger.LogInformation($"Fetching from Google Places API: {googlePlacesUrl}");

                    var googleRes = await client.GetAsync(googlePlacesUrl);

                    if (googleRes.IsSuccessStatusCode)
                    {
                        var googleContent = await googleRes.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                        var googlePlaces = JsonSerializer.Deserialize<List<GooglePlaceResponse>>(googleContent, options) ?? new();

                        restaurants = googlePlaces.Select(gp => new GooglePlaceSimpleDto
                        {
                            PlaceId = gp.PlaceId ?? "",
                            Name = gp.Name ?? "Chưa có tên",
                            Address = gp.FormattedAddress ?? gp.Vicinity ?? "Chưa có địa chỉ",
                            Rating = gp.Rating ?? 0,
                            PriceLevel = gp.PriceLevel,
                            OpeningHours = gp.OpeningHours?.WeekdayText != null && gp.OpeningHours.WeekdayText.Any()
                                ? string.Join("; ", gp.OpeningHours.WeekdayText)
                                : "Chưa có thông tin giờ mở cửa",
                            Types = gp.Types ?? new List<string>(),
                            Latitude = gp.Geometry?.Location?.Lat ?? 0,
                            Longitude = gp.Geometry?.Location?.Lng ?? 0
                        }).Take(15).ToList();

                        _logger.LogInformation($"Successfully mapped {restaurants.Count} restaurants from Google Places");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching from Google Places API");
                }

                // 3. Lấy review từ database (nếu có)
                Dictionary<string, List<ReviewDto>> reviewsByPlaceId = new();
                if (restaurants.Any())
                {
                    foreach (var restaurant in restaurants.Take(10))
                    {
                        try
                        {
                            var reviewUrl = $"{gatewayBase}/api/reviews/google/get-by-place-id?placeId={restaurant.PlaceId}&top=3";
                            var reviewRes = await client.GetAsync(reviewUrl);

                            if (reviewRes.IsSuccessStatusCode)
                            {
                                var reviewJson = await reviewRes.Content.ReadAsStringAsync();
                                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                                var reviews = JsonSerializer.Deserialize<List<ReviewDto>>(reviewJson, options);

                                if (reviews != null && reviews.Any())
                                {
                                    reviewsByPlaceId[restaurant.PlaceId] = reviews;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to get reviews for place {restaurant.PlaceId}");
                        }
                    }
                }

                // 4. Tạo prompt cho AI - YÊU CẦU RESPONSE THEO FORMAT JSON
                var sb = new StringBuilder();
                sb.AppendLine("⚠️ QUAN TRỌNG: Bạn PHẢI trả lời theo đúng định dạng bên dưới, không được thêm bớt gì và chỉ đề xuất tập trung vào quán ăn, cafe, nhà hàng, không nên đề xuất chỗ nào khác:\n");
                sb.AppendLine("```json");
                sb.AppendLine("{");
                sb.AppendLine("  \"message\": \"Câu trả lời chi tiết bằng tiếng Việt cho người dùng\",");
                sb.AppendLine("  \"recommendedPlaceIds\": [\"ChIJ...\", \"ChIJ...\", ...]");
                sb.AppendLine("}");
                sb.AppendLine("```\n");

                sb.AppendLine("=== YÊU CẦU CỦA NGƯỜI DÙNG ===");
                sb.AppendLine(prompt);
                sb.AppendLine();

                sb.AppendLine("=== VỊ TRÍ NGƯỜI DÙNG ===");
                sb.AppendLine($"Tọa độ: {lat}, {lng}");
                sb.AppendLine($"Bán kính: {radius}m (~{radius / 1000.0:F1}km)");
                sb.AppendLine();

                sb.AppendLine("=== SỞ THÍCH NGƯỜI DÙNG ===");
                sb.AppendLine(userPrefJson);
                sb.AppendLine();

                sb.AppendLine("=== DANH SÁCH QUÁN ĂN TỪ GOOGLE MAPS ===");

                if (restaurants.Any())
                {
                    for (int i = 0; i < restaurants.Count; i++)
                    {
                        var rest = restaurants[i];
                        sb.AppendLine($"\n{i + 1}. **{rest.Name}**");
                        sb.AppendLine($"   PlaceId: {rest.PlaceId}");
                        sb.AppendLine($"   Đánh giá: {rest.Rating:F1}⭐");
                        sb.AppendLine($"   Địa chỉ: {rest.Address}");

                        if (rest.PriceLevel.HasValue)
                        {
                            var priceDisplay = new string('$', rest.PriceLevel.Value + 1);
                            sb.AppendLine($"   Mức giá: {priceDisplay}");
                        }

                        if (rest.Types != null && rest.Types.Any())
                        {
                            sb.AppendLine($"   Loại: {string.Join(", ", rest.Types.Take(3))}");
                        }

                        if (!string.IsNullOrEmpty(rest.OpeningHours))
                        {
                            var hours = rest.OpeningHours.Split(';').Take(2);
                            sb.AppendLine($"   Giờ mở: {string.Join("; ", hours)}");
                        }

                        if (reviewsByPlaceId.TryGetValue(rest.PlaceId, out var reviews) && reviews.Any())
                        {
                            sb.AppendLine("   Review:");
                            foreach (var rv in reviews.Take(2))
                            {
                                sb.AppendLine($"     • \"{rv.Comment}\" - {rv.UserName} ({rv.Rating}⭐)");
                            }
                        }
                    }
                }
                else
                {
                    sb.AppendLine("Không tìm thấy quán nào.");
                }

                sb.AppendLine("\n=== HƯỚNG DẪN ===");
                sb.AppendLine("1. Phân tích yêu cầu và sở thích của người dùng");
                sb.AppendLine("2. Chọn 5 quán PHÙ HỢP NHẤT");
                sb.AppendLine("3. Trong field 'message': Viết câu trả lời chi tiết, thân thiện bằng tiếng Việt, ");
                sb.AppendLine("   - Giới thiệu từng quán ăn, cafe với tên, địa chỉ, đánh giá");
                sb.AppendLine("   - Giải thích tại sao phù hợp với yêu cầu");
                sb.AppendLine("   - Trích dẫn review nếu có");
                sb.AppendLine("4. Trong field 'recommendedPlaceIds': Liệt kê CHÍNH XÁC các PlaceId của quán bạn đề xuất");
                sb.AppendLine("Trước tiên, nếu user hỏi câu không liên quan đến quán ăn thì hãy trả lời đúng trọng tâm. " 
                    + "Nếu liên quan đến ăn uống, hãy linh hoạt gợi ý dựa vào vị trí gần (≤ 5km), " 
                    + "không cần quá chú trọng preference nếu trống. " 
                    + "Không được yêu cầu user mở Google Maps, không gọi API ngoài. " 
                    + "Phải trả về ít nhất 5 quán ăn hoặc cafe phổ biến, giá hợp lý cho sinh viên, " 
                    + "có đủ thông tin: tên, địa chỉ, giờ mở cửa, món nổi bật.");

                // 5. Gọi AI
                string aiResponse = "";
                List<string> recommendedPlaceIds = new();

                try
                {
                    var fullPrompt = sb.ToString();
                    _logger.LogInformation($"Sending prompt to AI (length: {fullPrompt.Length} chars)");

                    var rawAiResponse = await _geminiAIService.getChatResponse(fullPrompt);
                    _logger.LogInformation($"Raw AI Response: {rawAiResponse}");

                    // Parse JSON response từ AI
                    try
                    {
                        // Loại bỏ markdown code block nếu có
                        var jsonContent = rawAiResponse.Trim();
                        if (jsonContent.StartsWith("```json"))
                        {
                            jsonContent = jsonContent.Substring(7);
                        }
                        if (jsonContent.StartsWith("```"))
                        {
                            jsonContent = jsonContent.Substring(3);
                        }
                        if (jsonContent.EndsWith("```"))
                        {
                            jsonContent = jsonContent.Substring(0, jsonContent.Length - 3);
                        }
                        jsonContent = jsonContent.Trim();

                        var aiStructured = JsonSerializer.Deserialize<AIRecommendResponse>(jsonContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (aiStructured != null)
                        {
                            aiResponse = aiStructured.Message ?? rawAiResponse;
                            recommendedPlaceIds = aiStructured.RecommendedPlaceIds ?? new List<string>();
                            _logger.LogInformation($"Successfully parsed AI response. Recommended {recommendedPlaceIds.Count} places");
                        }
                        else
                        {
                            _logger.LogWarning("Failed to parse AI response as structured JSON");
                            aiResponse = rawAiResponse;
                            // Fallback: extract PlaceIds từ text
                            recommendedPlaceIds = ExtractPlaceIdsFromText(rawAiResponse, restaurants);
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Failed to parse AI JSON response, using fallback");
                        aiResponse = rawAiResponse;
                        // Fallback: extract PlaceIds từ text
                        recommendedPlaceIds = ExtractPlaceIdsFromText(rawAiResponse, restaurants);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling Gemini AI");
                    aiResponse = $"Xin lỗi, hệ thống AI đang gặp sự cố. Tuy nhiên, tôi đã tìm thấy {restaurants.Count} quán ăn gần bạn.";
                    // Return all restaurants as fallback
                    recommendedPlaceIds = restaurants.Select(r => r.PlaceId).ToList();
                }

                // 6. Filter restaurants theo PlaceIds được AI recommend
                var filteredRestaurants = restaurants
                    .Where(r => recommendedPlaceIds.Contains(r.PlaceId))
                    .ToList();

                _logger.LogInformation($"Filtered to {filteredRestaurants.Count} restaurants based on AI recommendations");

                // 7. Trả về response
                var response = new
                {
                    answer = aiResponse,
                    totalRestaurants = filteredRestaurants.Count,
                    totalAvailable = restaurants.Count,
                    source = "Google Places API",
                    searchRadius = $"{radius}m",
                    restaurants = filteredRestaurants.Select(r => new
                    {
                        placeId = r.PlaceId,
                        name = r.Name,
                        address = r.Address,
                        rating = r.Rating,
                        priceLevel = r.PriceLevel,
                        types = r.Types,
                        openingHours = r.OpeningHours,
                        mapUrl = $"https://www.google.com/maps/place/?q=place_id:{r.PlaceId}",
                        hasReviews = reviewsByPlaceId.ContainsKey(r.PlaceId)
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SmartRecommend");
                return StatusCode(500, new
                {
                    error = "Đã xảy ra lỗi khi xử lý yêu cầu",
                    message = ex.Message
                });
            }
        }

        // Helper method: Extract PlaceIds từ text nếu AI không trả JSON đúng format
        private List<string> ExtractPlaceIdsFromText(string text, List<GooglePlaceSimpleDto> availableRestaurants)
        {
            var placeIds = new List<string>();

            // Pattern để match PlaceId (thường bắt đầu bằng ChIJ)
            var placeIdPattern = @"ChIJ[\w-]+";
            var matches = Regex.Matches(text, placeIdPattern);

            foreach (Match match in matches)
            {
                var placeId = match.Value;
                if (availableRestaurants.Any(r => r.PlaceId == placeId) && !placeIds.Contains(placeId))
                {
                    placeIds.Add(placeId);
                }
            }

            // Nếu không tìm thấy PlaceId nào, fallback = extract dựa trên tên quán
            if (!placeIds.Any())
            {
                foreach (var restaurant in availableRestaurants)
                {
                    if (text.Contains(restaurant.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        placeIds.Add(restaurant.PlaceId);
                    }
                }
            }

            _logger.LogInformation($"Extracted {placeIds.Count} PlaceIds from text using fallback method");
            return placeIds;
        }
    }

    // DTO cho AI response
    
}