using UserService.API.Models.DTO;
using UserService.API.Models.Entity;
using UserService.API.Repository;

namespace UserService.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetUserAccount(string email, string password)
        {
            return await _userRepository.GetUserAccount(email, password);
        }

        public async Task<User> RegisterAsync(RegisterRequest req)
        {
            return await _userRepository.RegisterAsync(req);
        }

    }
}
