using Microsoft.EntityFrameworkCore;
using SocialService.API.Data.DBContext;
using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public class PostReactionRepository : IPostReactionRepository
    {
        private readonly Exe201SocialServiceDbContext _context;

        public PostReactionRepository(Exe201SocialServiceDbContext context)
        {
            _context = context;
        }

        public async Task<PostReaction?> GetUserReactionAsync(int userId, int postId)
        {
            return await _context.PostReactions
                .Include(r => r.ReactionType)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);
        }

        public async Task<IEnumerable<PostReaction>> GetAllReactionsByPostIdAsync(int postId)
        {
            return await _context.PostReactions
                .Include(r => r.ReactionType)
                .Where(r => r.PostId == postId)
                .ToListAsync();
        }

        public async Task AddReactionAsync(PostReaction reaction)
        {
            await _context.PostReactions.AddAsync(reaction);
        }

        public void RemoveReaction(PostReaction reaction)
        {
            _context.PostReactions.Remove(reaction);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
