using Microsoft.EntityFrameworkCore;
using SocialService.API.Data.DBContext;
using SocialService.API.Models.Entity;
using System.Linq.Dynamic.Core;

namespace SocialService.API.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly Exe201SocialServiceDbContext _context;

        public CommentRepository(Exe201SocialServiceDbContext context)
        {
            _context = context;
        }
        public async Task<List<Comment?>> GetCommentByPostIdAsync(int postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        public async Task AddComment(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

