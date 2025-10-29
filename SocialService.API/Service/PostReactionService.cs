using Microsoft.EntityFrameworkCore;
using SocialService.API.Data.DBContext;
using SocialService.API.Models.Entity;
using SocialService.API.Repository;

namespace SocialService.API.Service
{
    public class PostReactionService : IPostReactionService
    {
        private readonly IPostReactionRepository _repo;
        private readonly Exe201SocialServiceDbContext _context;

        public PostReactionService(IPostReactionRepository repo, Exe201SocialServiceDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        // ✅ Toggle reaction (like, love, haha...) — chỉ 1 loại / user
        public async Task<string> ToggleReactionAsync(int userId, int postId, string? reactionType)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);
            if (post == null)
                throw new Exception("Bài viết không tồn tại.");

            var existing = await _repo.GetUserReactionAsync(userId, postId);

            // ✅ NÊU reactionType là null/empty → XÓA reaction hiện tại
            if (string.IsNullOrWhiteSpace(reactionType))
            {
                if (existing != null)
                {
                    _repo.RemoveReaction(existing);
                    post.ReactionsCount = Math.Max(0, post.ReactionsCount - 1);
                    await _repo.SaveChangesAsync();
                    await _context.SaveChangesAsync();
                    return "Reaction removed";
                }
                return "No reaction to remove";
            }

            // ✅ Tìm reaction type từ database
            var type = await _context.ReactionTypes
                .FirstOrDefaultAsync(r =>
                    r.Label.ToUpper() == reactionType.ToUpper() ||
                    r.Code.ToUpper() == reactionType.ToUpper());

            if (type == null)
                throw new Exception($"Reaction type '{reactionType}' không tồn tại.");

            // ✅ Nếu user đã có reaction
            if (existing != null)
            {
                // Nếu cùng loại → XÓA (toggle off)
                if (existing.ReactionTypeId == type.ReactionTypeId)
                {
                    _repo.RemoveReaction(existing);
                    post.ReactionsCount = Math.Max(0, post.ReactionsCount - 1);
                    await _repo.SaveChangesAsync();
                    await _context.SaveChangesAsync();
                    return "Reaction removed";
                }

                // Nếu khác loại → UPDATE (không thay đổi count)
                existing.ReactionTypeId = type.ReactionTypeId;
                existing.CreatedAt = DateTime.UtcNow;
                await _repo.SaveChangesAsync();
                await _context.SaveChangesAsync();
                return $"Reaction changed to '{reactionType}'";
            }

            // ✅ Nếu chưa có → THÊM MỚI
            var newReaction = new PostReaction
            {
                PostId = postId,
                UserId = userId,
                ReactionTypeId = type.ReactionTypeId,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddReactionAsync(newReaction);
            post.ReactionsCount++;
            await _repo.SaveChangesAsync();
            await _context.SaveChangesAsync();
            return $"Reaction added: {reactionType}";
        }

        // ✅ Check user đã reaction bài viết hay chưa
        public async Task<object> CheckUserReactionAsync(int userId, int postId)
        {
            var reaction = await _repo.GetUserReactionAsync(userId, postId);
            if (reaction == null)
                return new { hasReacted = false };

            var type = await _context.ReactionTypes
                .FirstOrDefaultAsync(r => r.ReactionTypeId == reaction.ReactionTypeId);

            return new
            {
                hasReacted = true,
                reactionType = type?.Label ?? type?.Code ?? "Unknown"
            };
        }

        public async Task<object> GetPostReactionsSummaryAsync(int postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            if (post == null)
                throw new Exception("Bài viết không tồn tại.");

            var reactions = await _repo.GetAllReactionsByPostIdAsync(postId);

            var summary = reactions
                .GroupBy(r => r.ReactionType.Label)
                .ToDictionary(g => g.Key, g => g.Count());

            return new
            {
                postId,
                totalReactions = reactions.Count(),
                summary
            };
        }
    }
}
