using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialService.API.Models.DTO;
using SocialService.API.Models.Entity;
using SocialService.API.Service;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _service;
        private readonly ICloudinaryService _cloudinaryService;

        public PostsController(IPostService service, ICloudinaryService cloudinaryService)
        {
            _service = service;
            _cloudinaryService = cloudinaryService;
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

        [HttpGet("get-post-by-restaurant-id/{id}")]
        public async Task<IActionResult> GetPostByRestaurantId(int id)
        {
            try
            {
                var posts = await _service.GetAllPostByRestaurantId(id);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [Authorize]
        [HttpPost("upload-images")]
        public async Task<List<string>> UploadImagesAsync([FromForm] List<IFormFile> files)
        {
            var urls = await _cloudinaryService.UploadImagesAsync(files, "posts");
            return urls;
        }

        [Authorize]
        [HttpPost("create")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> CreatePost([FromForm] PostCreateDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Không tìm thấy user trong token." });

                int userId = int.Parse(userIdClaim);

                var (postId, googlePlaceId) = await _service.CreatePostAsync(dto, userId);
                return Ok(new
                {
                    message = "Tạo bài viết thành công.",
                    postId,
                    googlePlaceId
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("create-post-mobile")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> CreatePostMobile([FromBody] PostCreateDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Không tìm thấy user trong token." });

                int userId = int.Parse(userIdClaim);

                var (postId, googlePlaceId) = await _service.CreatePostAsync(dto, userId);
                return Ok(new
                {
                    message = "Tạo bài viết thành công.",
                    postId,
                    googlePlaceId
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("get-all-post-of-userId")]
        public async Task<IActionResult> GetPostsByUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "Không tìm thấy user trong token." });
                int userId = int.Parse(userIdClaim);
                var posts = await _service.GetPostsByUserIdAsync(userId);
                return Ok(posts);
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

        [Authorize]
        [HttpGet("get-all-post-of-restaurant-of-userId")]
        public async Task<List<Post>> GetAllPostOfRestaurantByUserId(int userId, int restaurantId)
        {
            return await _service.GetAllPostOfRestaurantIdByUserId(userId, restaurantId);
        }

        [HttpGet("count-post-by-day-last-14-days")]
        public async Task<IActionResult> GetPostCountByDayLast14Days()
        {
            var data = await _service.GetPostCountByDayLast14DaysAsync();
            return Ok(new
            {
                labels = data.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList(),
                values = data.Select(x => x.PostCount).ToList()
            });
        }

    }
}
