using Microsoft.EntityFrameworkCore;
using UserService.API.Data.DBContext;
using UserService.API.Models.DTO;
using UserService.API.Models.Entity;

namespace UserService.API.Repository
{
        public class UserRepository : IUserRepository
    {
        private readonly Exe201UserServiceDbContext _context;
        public UserRepository(Exe201UserServiceDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserAccount(string email, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == password && u.Status == "Active");
        }

        public async Task<User> RegisterAsync(RegisterRequest req)
        {
            // Kiểm tra email đã tồn tại
            if (await _context.Users.AnyAsync(x => x.Email == req.Email))
                throw new Exception("Email already exists.");

            var newUser = new User
            {
                FullName = req.FullName,
                Email = req.Email,
                PasswordHash = req.PasswordHash,
                BirthDate = req.BirthDate,
                CreatedAt = DateTime.UtcNow,
                Status = "Active",
                RoleId = 3,
                RoleName = "user"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }
      
    }
}
