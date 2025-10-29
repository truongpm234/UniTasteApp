using UserService.API.Models.DTO;
using UserService.API.Models.Entity;

namespace UserService.API.Services
{
    public interface IUserService
    {
        Task<User> GetUserAccount(string email, string password);
        Task<User> GetFullNameByUserIdAsync(int userId);
        Task<User> GetUserAccountByEmail(string email);
        Task<User> RegisterAsync(RegisterRequest req);
        Task<string?> SendResetPasswordEmailAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        string GenerateOtpCode(int length = 6);
        Task SendRegisterOtpEmailAsync(string email, string fullName, string otpCode);
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileDto dto);
        Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        bool IsStrongPassword(string password);
        Task<Dictionary<int, int>> CountUserRegisterByMonthAsync(int year);
        Task<int> CountAccountActiveAsync();
        Task<int> CountAccountInactiveAsync();
        Task<UserPreference> CreateUserPreferenceAsync(CreateUserPreferenceDto dto);
        Task<UserPreference?> GetUserPreferenceByUserIdAsync(int userId);
        Task<UserPreference?> UpdateUserPreferenceAsync(int userId, UpdateUserPreferenceDto dto);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByFullNameAsync(string fullName);
        Task<User?> GetUserByIdAsync(int userId);

    }
}