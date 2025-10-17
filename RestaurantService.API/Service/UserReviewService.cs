using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Repository;

namespace RestaurantService.API.Service
{
    public class UserReviewService : IUserReviewService
    {
        private readonly IUserReviewRepository _repository;
        public UserReviewService(IUserReviewRepository repository)
        {
            _repository = repository;
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
    }
}
