using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialService.API.Models.DTO;
using SocialService.API.Service;
using System.Security.Claims;

namespace SocialService.API.Controllers
{
    //[Route("api/social/posts")]
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _service;

        public PostsController(IPostService service)
        {
            _service = service;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllReviews()
        {
            var data = await _service.GetAllReviewsAsync();
            return Ok(data);
        }

        [Authorize]
        [HttpPost("create")]
        [RequestSizeLimit(50_000_000)] // cho phép upload file lớn hơn mặc định
        public async Task<IActionResult> CreatePost([FromForm] PostCreateDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Không tìm thấy user trong token." });

                int userId = int.Parse(userIdClaim);

                var id = await _service.CreatePostAsync(dto, userId);
                return Ok(new { message = "Tạo bài viết thành công.", postId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
