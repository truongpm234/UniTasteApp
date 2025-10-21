using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialService.API.Models.DTO;
using SocialService.API.Service;
using System.Security.Claims;

namespace SocialService.API.Controllers
{
    [Route("api/social/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateComment([FromBody] CommentCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { message = "Content cannot be empty." });

            try
            {
                // ✅ Lấy userId từ token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "User ID not found in token." });

                int userId = int.Parse(userIdClaim);

                // ✅ Gửi qua service
                var id = await _commentService.CreateComment(dto, userId);

                return Ok(new
                {
                    message = "Comment created successfully.",
                    commentId = id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
