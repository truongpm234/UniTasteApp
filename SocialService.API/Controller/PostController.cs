using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialService.API.Models.DTO;
using SocialService.API.Service;
using System.Security.Claims;

namespace SocialService.API.Controllers
{
    [Route("api/social/posts")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _service;

        public PostController(IPostService service)
        {
            _service = service;
        }

        [HttpGet("get-all-paged")]
        public async Task<IActionResult> GetAllReviewsPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 3)
        {
            try
            {
                var result = await _service.GetAllReviewsPagedAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("get-by-user")]
        public async Task<IActionResult> GetPostsByUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Không tìm thấy user trong token." });

                int userId = int.Parse(userIdClaim);
                var data = await _service.GetPostsByUserIdAsync(userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
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

        [Authorize]
        [HttpPut("update/{postId}")]
        [RequestSizeLimit(50_000_000)] // cho phép upload ảnh lớn
        public async Task<IActionResult> UpdatePost(int postId, [FromForm] PostUpdateDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Không tìm thấy user trong token." });

                int userId = int.Parse(userIdClaim);
                await _service.UpdatePostAsync(postId, userId, dto);

                return Ok(new { message = "Cập nhật bài viết thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("delete/{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Không tìm thấy user trong token." });

                int userId = int.Parse(userIdClaim);

                await _service.DeletePostAsync(postId, userId);
                return Ok(new { message = "Xóa bài viết thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
