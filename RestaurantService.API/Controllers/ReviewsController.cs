using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.API.Service;

namespace RestaurantService.API.Controllers
{
    [Route("api/reviews/google")]
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [Authorize]
        [HttpGet("get-all-review")]
        public async Task<IActionResult> GetAllReviewsAsync()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            int count = reviews?.Count ?? 0;

            if (reviews == null || reviews.Count == 0)
            {
                return NotFound("No review found.");
            }
            return Ok(new
            {
                count,
                reviews
            });
        }

        [Authorize]
        [HttpPost("get-top-reviews-multiple")]
        public async Task<IActionResult> GetTopReviewsMultiple([FromBody] List<int> restaurantIds, int top = 4)
        {
            var result = new List<object>();
            foreach (var id in restaurantIds)
            {
                var reviews = await _reviewService.GetTopReviewsByRestaurantIdAsync(id, top);
                result.Add(new { RestaurantId = id, Reviews = reviews });
            }
            return Ok(result);
        }

    }
}
