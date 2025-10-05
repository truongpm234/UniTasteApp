using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.API.Models.DTO;
using UserService.API.Models.Entity;
using UserService.API.Services;
using static UserService.API.Repository.UserRepository;

namespace UserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        public UsersController(IConfiguration config, IUserService userService, IEmailService emailService)
        {
            _config = config;
            _userService = userService;
            _emailService = emailService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.GetUserAccount(request.Email, request.Password);

            if (user == null)
                return Unauthorized();

            var token = GenerateJSONWebToken(user);

            return Ok(new
            {
                token,
                fullName = user.FullName,
                email = user.Email
            });
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
            new(ClaimTypes.NameIdentifier, systemUserAccount.UserId.ToString()), 
            new(ClaimTypes.Email, systemUserAccount.Email),
            new(ClaimTypes.Role, systemUserAccount.RoleId.ToString()),
                },
                expires: DateTime.Now.AddMinutes(500000),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public sealed record LoginRequest(string Email, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            // Check if email already exists in the database
            var existingUser = await _userService.GetUserAccountByEmail(req.Email);
            if (existingUser != null)
            {
                return BadRequest(new { status = false, message = "Email already exists." });
            }

            var otpCode = _userService.GenerateOtpCode();

            RegisterOtpMemory.Pending[req.Email] = new RegisterVerification
            {
                Email = req.Email,
                FullName = req.FullName,
                PasswordHash = req.PasswordHash,
                BirthDate = req.BirthDate.ToDateTime(TimeOnly.MinValue),
                OtpCode = otpCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(120)
            };

            await _userService.SendRegisterOtpEmailAsync(req.Email, req.FullName, otpCode);

            return Ok(new { status = true, message = "OTP sent to your email. Please verify to complete registration." });
        }


        [HttpPost("verify-register")]
        public async Task<IActionResult> VerifyRegister([FromBody] RegisterVerifyRequest req)
        {
            // Kiểm tra OTP
            if (!RegisterOtpMemory.Pending.TryGetValue(req.Email, out var pending) ||
                pending.ExpiresAt < DateTime.UtcNow ||
                pending.OtpCode != req.OtpCode)
            {
                return BadRequest(new { error = "OTP is invalid or expired" });
            }

 
                // Chuẩn bị thông tin để lưu vào DB
                var registerRequest = new RegisterRequest
                {
                    Email = pending.Email,
                    FullName = pending.FullName,
                    PasswordHash = pending.PasswordHash,
                    BirthDate = DateOnly.FromDateTime(pending.BirthDate)
                };
                var user = await _userService.RegisterAsync(registerRequest);           
                RegisterOtpMemory.Pending.Remove(req.Email);

                return Ok(new { status = true, message = "Register successful", user = new { user.UserId, user.Email, user.FullName } });
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
    public class RegisterVerifyRequest
    {
        public string Email { get; set; }
        public string OtpCode { get; set; }
    }
    public class ConfirmResetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
