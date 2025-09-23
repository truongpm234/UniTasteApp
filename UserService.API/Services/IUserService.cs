using UserService.API.Models.DTO;
using UserService.API.Models.Entity;

namespace UserService.API.Services
{
    public interface IUserService
    {
        Task<User> GetUserAccount(string email, string password);
        Task<User> RegisterAsync(RegisterRequest req);
    }
}