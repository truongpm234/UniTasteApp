using Microsoft.EntityFrameworkCore;
using UserService.API.Data.DBContext;
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

        

    }
}
