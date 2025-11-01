using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialService.API.Models.Entity;
using SocialService.API.Service;

namespace SocialService.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserFeedbacksController : ControllerBase
    {
        private readonly IUserFeedbackService _service;
        public UserFeedbacksController(IUserFeedbackService service)
        {
            _service = service;
        }

        [HttpPost("create-userFeedBack")]
        public async Task<IActionResult> Add([FromBody] UserFeedback feedback)
        {
            if (feedback.Rating < 0.5 || feedback.Rating > 5)
                return BadRequest("Rating must be from 0.5 to 5.");
            feedback.CreatedAt = DateTime.UtcNow;
            var added = await _service.AddFeedbackAsync(feedback);
            return Ok(added);
        }

        [HttpGet("get-all-user-feedback")]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("average-rating")]
        public async Task<IActionResult> GetAverageRating()
            => Ok(await _service.GetAverageRatingAsync());
    }

}
