using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;

namespace RestaurantService.API.Service
{
    public interface IUserReviewService
    {
        Task<List<Review>> GetReviewsByRestaurantIdAsync(int restaurantId);
        Task<Review> AddReviewAsync(ReviewCreateDto dto);
    }
}