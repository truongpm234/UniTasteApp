using UserService.API.Models.DTO;
using UserService.API.Models.Entity;

namespace UserService.API.Services
{
    public interface IUserService
    {
        Task<User> GetUserAccount(string email, string password);
        Task<User> GetUserAccountByEmail(string email);
        Task<User> RegisterAsync(RegisterRequest req);
        Task<string?> SendResetPasswordEmailAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        string GenerateOtpCode(int length = 6);
        Task SendRegisterOtpEmailAsync(string email, string fullName, string otpCode);
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileDto dto);


    }
}