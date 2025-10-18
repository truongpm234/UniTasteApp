using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Repository;

namespace RestaurantService.API.Service
{
    public class UserReviewService : IUserReviewService
    {
        private readonly IUserReviewRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        public UserReviewService(IUserReviewRepository repository, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
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
            // Gọi qua Gateway
            var client = _httpClientFactory.CreateClient();
            var url = $"http://localhost:8001/api/userreviews/user-has-reviewed?userId={userId}&restaurantId={restaurantId}";

            var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
                throw new Exception("Không gọi được API kiểm tra review!");

            var json = await resp.Content.ReadAsStringAsync();

            // Dùng Newtonsoft.Json hoặc System.Text.Json đều được
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            // Nếu để response là { "hasReviewed": true }
            return obj.hasReviewed == true;
        }

        public async Task<bool> UserHasReviewedAsync(int userId, int restaurantId)
        {
            return await _repository.ExistsReviewByUserAsync(userId, restaurantId);
        }

    }
}
