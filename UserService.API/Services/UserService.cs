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

        public async Task<User> RegisterAsync(RegisterRequest req)
        {
            return await _userRepository.RegisterAsync(req);
        }
        public async Task<bool> SendResetPasswordEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return false;

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
            var passwordResetToken = new PasswordResetToken
            {
                UserId = user.UserId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                IsUsed = false
            };
            await _userRepository.AddAsync(passwordResetToken);
            await _userRepository.SaveChangesAsync();

            // *** Gửi thẳng mã token ***
            var body = $"Mã xác thực đặt lại mật khẩu của bạn là:<br><b>{token}</b><br>(Mã này sẽ hết hạn sau 30 phút.)";
            await _emailService.SendEmailAsync(email, "Password Reset Verification Code", body);

            return true;
        }


        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var tokenObj = await _userRepository.GetByTokenAsync(token);
            if (tokenObj == null || tokenObj.IsUsed == true) return false;

            var user = tokenObj.User;
            if (user == null) return false;

            user.PasswordHash = newPassword;
            tokenObj.IsUsed = true;
            await _userRepository.SaveChangesAsync();

            return true;
        }
    }
}
