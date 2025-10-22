using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialService.API.Service;
using System.Security.Claims;

namespace SocialService.API.Controllers
{
    //[Route("api/social/reactions")]
    [Route("api/[controller]")]
    [ApiController]
    public class PostReactionsController : ControllerBase
    {
        private readonly IPostReactionService _service;

        public PostReactionsController(IPostReactionService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost("make-creaction")]
        public async Task<IActionResult> ToggleReaction(int postId, string reactionType)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token." });

                int userId = int.Parse(userIdClaim);
                var message = await _service.ToggleReactionAsync(userId, postId, reactionType);

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("Is-make-reaction-for-post")]
        public async Task<IActionResult> CheckReaction(int postId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Không tìm thấy userId trong token." });

                int userId = int.Parse(userIdClaim);
                var result = await _service.CheckUserReactionAsync(userId, postId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [Authorize]
        [HttpGet("summary-amount-react")]
        public async Task<IActionResult> GetReactionsSummary(int postId)
        {
            try
            {
                var result = await _service.GetPostReactionsSummaryAsync(postId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
