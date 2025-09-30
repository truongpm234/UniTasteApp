using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.API.Models.DTO;
using UserService.API.Models.Entity;
using UserService.API.Services;

namespace UserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUserService _userService;

        public UsersController(IConfiguration config, IUserService userService)
        {
            _config = config;
            _userService = userService;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _userService.GetUserAccount(request.Email, request.Password);

            if (user == null || user.Result == null)
                return Unauthorized();

            var token = GenerateJSONWebToken(user.Result);

            return Ok(token);
        }

        private string GenerateJSONWebToken(User systemUserAccount)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                new Claim[]
                {
            new(ClaimTypes.NameIdentifier, systemUserAccount.UserId.ToString()), // ✅ thêm UserId
            new(ClaimTypes.Email, systemUserAccount.Email),
            new(ClaimTypes.Role, systemUserAccount.RoleId.ToString()),
                },
                expires: DateTime.Now.AddMinutes(5000),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public sealed record LoginRequest(string Email, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            try
            {
                var user = await _userService.RegisterAsync(req);
                return Ok(new { user.UserId, user.Email, user.FullName, user.BirthDate, user.RoleId, user.RoleName });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("request-reset-password")]
        public async Task<IActionResult> RequestReset([FromBody] RequestResetPasswordDto dto)
        {
            var ok = await _userService.SendResetPasswordEmailAsync(dto.Email);
            if (!ok)
                return Ok(new { status = "false", message = "Email not found!" }); 
            return Ok(new { status = "true", message = "Please check email to get OTP" });
        }

        [HttpPost("confirm-reset-password")]
        public async Task<IActionResult> ConfirmReset([FromBody] ConfirmResetPasswordDto dto)
        {
            var ok = await _userService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!ok)
                return Ok(new { status = "false", message = "Invalid or expired token!" });
            return Ok(new { status = "true", message = "Password changed successfully." });
        }

    }

    public class RequestResetPasswordDto
    {
        public string Email { get; set; }
    }

    public class ConfirmResetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
