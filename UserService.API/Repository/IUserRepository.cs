using UserService.API.Models.DTO;
using UserService.API.Models.Entity;

namespace UserService.API.Repository
{
    public interface IUserRepository
    {
        Task<User> GetUserAccount(string email, string password);
        Task<User> GetUserAccountByEmail(string email);
        Task<User> RegisterAsync(RegisterRequest req);
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        Task AddAsync(PasswordResetToken token);
        Task SaveChangesAsync();
        Task<User?> GetByEmailAsync(string email);
        string GenerateOtpCode(int length = 6);
        Task<User?> GetUserByIdAsync(int userId);
        Task UpdateAsync(User user);

    }
}