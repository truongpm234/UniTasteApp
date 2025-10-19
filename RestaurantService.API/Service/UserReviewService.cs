using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Repository;

namespace RestaurantService.API.Service
{
    public class UserReviewService : IUserReviewService
    {
        private readonly IUserReviewRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        public UserReviewService(IUserReviewRepository repository, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<List<Review>> GetReviewsByRestaurantIdAsync(int restaurantId)
        {
            return await _repository.GetReviewsByRestaurantIdAsync(restaurantId);
        }

        public async Task<Review> AddReviewAsync(ReviewCreateDto dto)
        {
            var review = new Review
            {
                RestaurantId = dto.RestaurantId,
                UserName = dto.UserName,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };
            return await _repository.AddReviewAsync(review);
        }

        public async Task<bool> CheckUserReviewedAsync(int userId, int restaurantId)
        {
            var client = _httpClientFactory.CreateClient();

            // Lấy baseUrl theo thứ tự ưu tiên: ENV > appsettings > fallback
            var gatewayBase = _config["GATEWAY_BASEURL"];

            if (string.IsNullOrWhiteSpace(gatewayBase))
            {
                // Nếu không có thì fallback về local
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() ?? "production";
                if (env == "development")
                    gatewayBase = "http://localhost:8001";
                else
                    gatewayBase = "https://apigateway-5s3w.onrender.com";
            }

            // Build URL
            var url = $"{gatewayBase}/api/userreviews/user-has-reviewed?userId={userId}&restaurantId={restaurantId}";

            var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
                throw new Exception("Không gọi được API kiểm tra review!");

            var json = await resp.Content.ReadAsStringAsync();

            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            return obj.hasReviewed == true;
        }
        //public async Task<bool> CheckUserReviewedAsync(int userId, int restaurantId)
        //{
        //    // Gọi qua Gateway
        //    var client = _httpClientFactory.CreateClient();
        //    var url = $"http://localhost:8001/api/userreviews/user-has-reviewed?userId={userId}&restaurantId={restaurantId}";

        //    var resp = await client.GetAsync(url);
        //    if (!resp.IsSuccessStatusCode)
        //        throw new Exception("Không gọi được API kiểm tra review!");

        //    var json = await resp.Content.ReadAsStringAsync();

        //    // Dùng Newtonsoft.Json hoặc System.Text.Json đều được
        //    var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
        //    // Nếu để response là { "hasReviewed": true }
        //    return obj.hasReviewed == true;
        //}

        public async Task<bool> UserHasReviewedAsync(int userId, int restaurantId)
        {
            return await _repository.ExistsReviewByUserAsync(userId, restaurantId);
        }

    }
}
