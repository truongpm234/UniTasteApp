using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialService.API.Models.DTO;
using SocialService.API.Service;
using System.Security.Claims;

namespace SocialService.API.Controllers
{
    [Route("api/social/share")]
    [ApiController]
    public class PostShareController : ControllerBase
    {
        private readonly IPostShareService _service;
        public PostShareController(IPostShareService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> SharePost([FromBody] PostShareCreateDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "User not found in token" });

                int userId = int.Parse(userIdClaim);

                var id = await _service.SharePostAsync(dto, userId);

                return Ok(new { message = "Post shared successfully", shareId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
