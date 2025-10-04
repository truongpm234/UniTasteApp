using Org.BouncyCastle.Crypto.Generators;
using System.Security.Cryptography;
using UserService.API.Models.DTO;
using UserService.API.Models.Entity;
using UserService.API.Repository;

using static Org.BouncyCastle.Math.EC.ECCurve;

namespace UserService.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;


        public UserService(IUserRepository userRepository, IEmailService emailService, IConfiguration config)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _config = config;


        }

        public async Task<User> GetUserAccount(string email, string password)
        {
            return await _userRepository.GetUserAccount(email, password);
        }
        public async Task<User> GetUserAccountByEmail(string email)
        {
            return await _userRepository.GetUserAccountByEmail(email);
        }

        public async Task<User> RegisterAsync(RegisterRequest req)
        {
            var user = await _userRepository.RegisterAsync(req);
            return user;
        }

        public async Task SendRegisterOtpEmailAsync(string email, string fullName, string otpCode)
        {
            var body = $@"
<table style='width:100%;max-width:480px;margin:auto;
              font-family:Segoe UI, Arial, sans-serif;
              border-radius:12px;background:#ffffff;
              box-shadow:0 2px 12px rgba(0,0,0,0.1);
              border:1px solid #eee'>
  <tr>
    <td style='padding:32px 24px;text-align:center'>
      <!-- Logo đơn giản, không có background -->
      <img src='https://sv2.anhsieuviet.com/2025/09/27/unitaste.jpg' 
           width='160' alt='UniTaste' style='margin-bottom:20px;display:block;margin-left:auto;margin-right:auto;' />
      
      <h2 style='margin:0 0 16px 0;color:#1181FF;
                 font-size:22px;font-weight:600'>
          Xác minh tài khoản UniTaste
      </h2>
      <p style='font-size:15px;color:#444;margin:0 0 20px 0'>
          Xin chào {fullName}, <br/>
          Đây là mã OTP để hoàn tất việc đăng ký tài khoản của bạn:
      </p>
      <div style='background:#1181FF;color:#fff;border-radius:8px;
                  display:inline-block;padding:14px 28px;
                  font-size:24px;letter-spacing:6px;
                  font-weight:bold;margin:20px 0'>
          {otpCode}
      </div>
      <p style='font-size:14px;color:#666;margin:24px 0 0 0;line-height:1.6'>
          Mã sẽ <b>hết hạn sau 15 phút</b>.<br/>
          Vui lòng nhập mã này trong ứng dụng để kích hoạt tài khoản.
      </p>
    </td>
  </tr>
</table>";

            await _emailService.SendEmailAsync(email, "UniTaste - Account Verification Code", body);
        }

        public async Task<bool> SendResetPasswordEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return false;

            var otpCode = GenerateOtpCode(6);

            var passwordResetToken = new PasswordResetToken
            {
                UserId = user.UserId,
                Token = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                IsUsed = false
            };
            await _userRepository.AddAsync(passwordResetToken);
            await _userRepository.SaveChangesAsync();

            // Gửi email OTP code
            var body = $@"
<table style='width:100%;max-width:420px;margin:auto;font-family:sans-serif;border-radius:12px;background:#fff;box-shadow:0 1px 8px #0002'>
  <tr>
    <td style='padding:36px 24px 20px 24px;text-align:center'>
      <img src='https://sv2.anhsieuviet.com/2025/09/27/unitaste.jpg' width='185' style='margin-bottom:10px;' alt='UniTaste' />
      <h2 style='margin:0 0 16px 0;color:#1181FF;font-size:22px;font-weight:bold'>Reset Your UniTaste Password</h2>
      <p style='font-size:15px;margin:0 0 22px 0;color:#333'>Xin chào,</p>
      <p style='font-size:15px;margin:0 0 16px 0'>Đây là mã xác thực đặt lại mật khẩu (OTP code) của bạn:</p>
      <div style='background:#F2F6FF;border-radius:8px;display:inline-block;padding:16px 28px;font-size:26px;letter-spacing:6px;font-weight:bold;color:#246'> {otpCode} </div>
      <p style='font-size:14px;margin:24px 0 0 0;color:#555'>Mã sẽ hết hạn sau <b>30 phút</b>. Nếu không phải bạn yêu cầu, hãy bỏ qua email này.</p>
    </td>
  </tr>
</table>
";

            await _emailService.SendEmailAsync(email, "UniTaste - Password Reset Verification Code", body);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var tokenObj = await _userRepository.GetByTokenAsync(token);
            if (tokenObj == null || tokenObj.IsUsed == true || tokenObj.ExpiresAt < DateTime.UtcNow)
                return false;

            var user = tokenObj.User;
            if (user == null) return false;

            user.PasswordHash = newPassword;
            tokenObj.IsUsed = true;
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public string GenerateOtpCode(int length = 6)
        {
            return _userRepository.GenerateOtpCode(length);
        }

    }
}
