using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialService.API.Models.DTO;
using SocialService.API.Service;
using System.Security.Claims;

namespace SocialService.API.Controllers
{
    //[Route("api/social/comments")]
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }
        [Authorize]
        [HttpPost("create-comment-for-post")]
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
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
