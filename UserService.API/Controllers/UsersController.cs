using Microsoft.AspNetCore.Authorization;
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
        private readonly IFirebaseStorageService _firebaseStorageService;
        public UsersController(IConfiguration config, IUserService userService, IEmailService emailService, IFirebaseStorageService firebaseStorageService)
        {
            _config = config;
            _userService = userService;
            _emailService = emailService;
            _firebaseStorageService = firebaseStorageService;
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
                userId = user.UserId,
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
            var otpCode = await _userService.SendResetPasswordEmailAsync(dto.Email);
            if (otpCode == null)
                return Ok(new { status = "false", message = "Email not found!" });

            // Trả về cả OTP trong response
            return Ok(new
            {
                status = "true",
                message = "Please check email to get OTP",
                otp = otpCode
            });
        }


        [HttpPost("confirm-reset-password")]
        public async Task<IActionResult> ConfirmReset([FromBody] ConfirmResetPasswordDto dto)
        {
            var ok = await _userService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!ok)
                return Ok(new { status = "false", message = "Invalid or expired token!" });
            return Ok(new { status = "true", message = "Password changed successfully." });
        }

        [Authorize]
        [HttpGet("get-profile-user-by-id/{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var profile = await _userService.GetUserProfileAsync(userId);
            if (profile == null) return NotFound("User not found");
            return Ok(profile);
        }

        [Authorize]
        [HttpPost("upload-avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
                return BadRequest("No file uploaded.");

            var url = await _firebaseStorageService.UploadFileAsync(avatarFile, "avatars");
            return Ok(new { avatarUrl = url });
        }

        [Authorize]
        [HttpPut("update-profile-user-by-id/{userId}")]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UpdateUserProfileDto dto)
        {
            var result = await _userService.UpdateUserProfileAsync(userId, dto);
            if (!result) return NotFound("User not found");
            return Ok("Profile updated successfully");
        }

        [Authorize]
        [HttpPost("create-user-preference")]
        public async Task<IActionResult> CreateUserPreference([FromBody] CreateUserPreferenceDto dto)
        {
            if (dto.UserId == 0)
                return BadRequest("UserId is required!");

            var result = await _userService.CreateUserPreferenceAsync(dto);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("get-user-preference-by-userid/{userId}")]
        public async Task<IActionResult> GetUserPreferenceByUserId(int userId)
        {
            var result = await _userService.GetUserPreferenceByUserIdAsync(userId);
            return result != null ? Ok(result) : NotFound();
        }

        [Authorize]
        [HttpPut("update-profile-user-by-userid/{userId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile(int userId, [FromForm] UpdateUserProfileForm form)
        {
            string? avatarUrl = null;
            if (form.AvatarFile != null && form.AvatarFile.Length > 0)
            {
                avatarUrl = await _firebaseStorageService.UploadFileAsync(form.AvatarFile, "avatars");
            }

            // 2. Tạo DTO cho update
            var dto = new UpdateUserProfileDto
            {
                FullName = form.FullName,
                Bio = form.Bio,
                Gender = form.Gender,
                BirthDate = form.BirthDate,
                PhoneNumber = form.PhoneNumber,
                AvatarUrl = avatarUrl
            };

            var result = await _userService.UpdateUserProfileAsync(userId, dto);
            if (!result) return NotFound("User not found");
            return Ok("Profile updated successfully");
        }

        [Authorize]
        [HttpPut("update-user-preference/{userId}")]
        public async Task<IActionResult> UpdateUserPreference(int userId, [FromBody] UpdateUserPreferenceDto dto)
        {
            var result = await _userService.UpdateUserPreferenceAsync(userId, dto);
            if (result == null)
                return NotFound("User preference not found to update.");
            return Ok(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var (success, errorMsg) = await _userService.ChangePasswordAsync(dto.UserId, dto.OldPassword, dto.NewPassword);
            if (!success)
                return BadRequest(errorMsg);
            return Ok("Đổi mật khẩu thành công.");
        }
        [Authorize]
        [HttpGet("count-register-by-month/{year}")]
        public async Task<IActionResult> CountRegisterByMonth(int year)
        {
            var result = await _userService.CountUserRegisterByMonthAsync(year);

            // Đảm bảo có đủ 12 tháng, gán 0 nếu không có dữ liệu
            var fullResult = Enumerable.Range(1, 12)
                .ToDictionary(m => m, m => result.ContainsKey(m) ? result[m] : 0);

            return Ok(new
            {
                year,
                data = fullResult
            });
        }

        [HttpGet("count-active")]
        public async Task<IActionResult> CountActiveAccounts()
        {
            var count = await _userService.CountAccountActiveAsync();
            return Ok(new { status = "Active", total = count });
        }

        [HttpGet("count-inactive")]
        public async Task<IActionResult> CountInactiveAccounts()
        {
            var count = await _userService.CountAccountInactiveAsync();
            return Ok(new { status = "Inactive", total = count });
        }



    }
}
