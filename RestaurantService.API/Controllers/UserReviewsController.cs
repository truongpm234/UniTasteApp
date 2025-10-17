using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Service;

namespace RestaurantService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserReviewsController : ControllerBase
    {
        private readonly IUserReviewService _reviewService;
        public UserReviewsController(IUserReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: api/UserReviews/{restaurantId}
        [Authorize]
        [HttpGet("get-all-review-by-restaurantId-{restaurantId}")]
        public async Task<ActionResult<List<Review>>> GetReviews(int restaurantId)
        {
            var reviews = await _reviewService.GetReviewsByRestaurantIdAsync(restaurantId);
            return Ok(reviews);
        }

        // POST: api/UserReviews
        [Authorize]
        [HttpPost("add-review-by-userId-with-restaurant-id")]
        public async Task<ActionResult<Review>> PostReview([FromBody] ReviewCreateDto review)
        {
            if (string.IsNullOrWhiteSpace(review.UserName) || review.RestaurantId == 0)
                return BadRequest("UserName and RestaurantId are required.");

            var created = await _reviewService.AddReviewAsync(review);
            return Ok(created);
        }
    }
}
