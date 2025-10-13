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
        public async Task<User> GetUserAccountByEmail(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
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
        public static class RegisterOtpMemory
        {
            public static Dictionary<string, RegisterVerification> Pending = new();
        }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token)
        {
            return await _context.PasswordResetTokens.Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && t.IsUsed == false && t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task AddAsync(PasswordResetToken token)
        {
            await _context.PasswordResetTokens.AddAsync(token);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public string GenerateOtpCode(int length = 6)
        {
            var random = new Random();
            string code = "";
            for (int i = 0; i < length; i++)
                code += random.Next(0, 10); 
            return code;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<int, int>> CountUserRegisterByMonthAsync(int year)
        {
            return await _context.Users
                .Where(u => u.CreatedAt.HasValue && u.CreatedAt.Value.Year == year)
                .GroupBy(u => u.CreatedAt.Value.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Month, g => g.Count);
        }

        public async Task<int> CountAccountActiveAsync()
        {
            return await _context.Users.CountAsync(u => u.Status == "Active");
        }

        public async Task<int> CountAccountInactiveAsync()
        {
            return await _context.Users.CountAsync(u => u.Status == "Inactive");
        }



    }
}
