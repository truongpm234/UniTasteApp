using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public AIController(IGeminiAIService geminiAIService, IHttpClientFactory httpClientFactory, IUserService userService)
        {
            _geminiAIService = geminiAIService;
            _httpClientFactory = httpClientFactory;
            _userService = userService;
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

            //var gatewayBase = "https://apigateway-5s3w.onrender.com";
            var gatewayBase = Environment.GetEnvironmentVariable("GATEWAY_BASEURL")
    ?? (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
        ? "http://localhost:8001"
        : "https://apigateway-5s3w.onrender.com");

            // 1. Lấy preference user
            var userPrefRes = await client.GetAsync($"{gatewayBase}/api/users/get-user-preference-by-userid/{userId}");
            if (!userPrefRes.IsSuccessStatusCode)
                return BadRequest("Không lấy được preference người dùng!");
            var userPrefJson = await userPrefRes.Content.ReadAsStringAsync();

            // 2. Lấy 15 quán gần nhất
            var resRes = await client.GetAsync($"{gatewayBase}/api/restaurants/get-nearest-restaurants?lat={lat}&lng={lng}&limit=15");
            if (!resRes.IsSuccessStatusCode)
                return BadRequest("Không lấy được danh sách quán gần nhất! Vui lòng điền thông tin về nhu cầu của bạn.");
            var restaurantJson = await resRes.Content.ReadAsStringAsync();
            var restaurants = System.Text.Json.JsonSerializer.Deserialize<List<RestaurantDto>>(restaurantJson);

            if (restaurants == null || restaurants.Count == 0)
                return Ok("Không tìm thấy quán phù hợp gần bạn!");

            // 3. Lấy top review mỗi quán (POST list id sang API review)
            var restIds = restaurants.Select(r => r.RestaurantId).ToList();
            var reviewContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(restIds),
                Encoding.UTF8, "application/json"
            );
            var reviewRes = await client.PostAsync(
                $"{gatewayBase}/api/reviews/google/get-top-reviews-multiple?top=4", reviewContent
            );
            if (!reviewRes.IsSuccessStatusCode)
                return BadRequest("Không lấy được review!");

            var reviewJson = await reviewRes.Content.ReadAsStringAsync();
            var reviewDict = System.Text.Json.JsonSerializer.Deserialize<List<ReviewGroupDto>>(reviewJson);

            var sb = new StringBuilder();
            sb.AppendLine("[USER REQUEST]: " + prompt);
            sb.AppendLine($"[USER LOCATION]: Latitude={lat}, Longitude={lng}");
            sb.AppendLine("[USER PREFERENCE]: " + userPrefJson);
            sb.AppendLine("[NEAREST RESTAURANTS]:");

            foreach (var rest in restaurants)
            {
                sb.AppendLine($"- {rest.Name} ({rest.GoogleRating ?? 0:F1}★):");
                var reviews = reviewDict?.FirstOrDefault(x => x.RestaurantId == rest.RestaurantId)?.Reviews;
                if (reviews != null)
                {
                    foreach (var rv in reviews.Take(4))
                        sb.AppendLine($"    • \"{rv.Comment}\" - {rv.UserName} ({rv.Rating}★)");
                }
            }
            sb.AppendLine("Trươc tiên nếu user hỏi các câu hỏi không liên quan đến gợi ý quán ăn thì hãy trả lời câu hỏi của họ, " +
                "nếu user hỏi những câu hỏi có liên quan " +
                "đến những việc liên quan đến nhu cầu ăn uống giair trí, không cần quá chú trọng vào nhu cầu đã có mà linh hoạt gợi ý, " +
                "chủ yếu cần các quán ở vị trí gần là được, vị trí các quán nên gợi ý ở gần vị trí kinh độ và vĩ độ người dùng nhập trong bán kính 5km," +
                " không nên xa quá 5km, không được gợi ý dùng các tool, hay " +
                "web khác hay các dịch vụ bên khác để gợi ý, mà chỉ trả lời ngay tại đây và cho ra quán cụ thể, nếu thật sự không có thì " +
                "bạn có thể tự chọn quán dựa trên dữ liệu của google và cho ra kết quả, không được trả về response là dựa vào các dịch vụ google hay các bên thứ 3 khác, " +
                "mà chỉ nói là 'dựa vào dữ liệu của chúng tôi'. " +
                "Lưu ý quan trọng là khi trả về kết quả thì cần có các thông tin về tên quán, địa chỉ quán, thời gian hoạt động và sẽ có những món gì và lưu ý là " +
                "nên gợi ý những nơi sinh viên thường đến, hạn chế gợi ý những nơi sang trọng và giá cả đắt đỏ. Kết quả cho ra ít nhất có thông tin 5 quán ăn hoặc cafe hoặc cả hai," +
                " không được yêu cầu user dùng google maps để tìm địa chỉ mà phải cho ra địa chỉ cụ thể, nếu không tìm được địa chỉ cụ thể thì không nên đưa ra gợi ý.");

            // 5. Gửi prompt sang Gemini
            var aiResponse = await _geminiAIService.getChatResponse(sb.ToString());


            return Ok(new { answer = aiResponse });
        }
    }
}
