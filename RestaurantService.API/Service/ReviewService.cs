using RestaurantService.API.Models.Entity;
using RestaurantService.API.Models.GooglePlaces;
using RestaurantService.API.Repository;
using System.Collections.Generic;

namespace RestaurantService.API.Service
{
    public class ReviewService: IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task AddOrUpdateReviewsAsync(int restaurantId, List<GoogleReview> googleReviews)
        {
            await _reviewRepository.AddOrUpdateReviewsAsync(restaurantId, googleReviews);
        }

        public async Task<List<Review>> GetAllReviewsAsync()
        {
            return await _reviewRepository.GetAllReviewsAsync();
        }

        public async Task<List<Review>> GetTopReviewsByRestaurantIdAsync(int restaurantId, int top = 4)
        {
            return await _reviewRepository.GetTopReviewsByRestaurantIdAsync(restaurantId, top);
        }   //AI


    }
}
