using UserService.API.Models.DTO;
using UserService.API.Models.Entity;

namespace UserService.API.Repository
{
    public interface IUserRepository
    {
        Task<User> GetUserAccount(string email, string password);
        Task<User> RegisterAsync(RegisterRequest req);

    }
}