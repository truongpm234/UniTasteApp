using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public interface IPostReactionRepository
    {
        Task<IEnumerable<PostReaction>> GetAllReactionsByPostIdAsync(int postId);
        Task<PostReaction?> GetUserReactionAsync(int userId, int postId);
        Task AddReactionAsync(PostReaction reaction);
        void RemoveReaction(PostReaction reaction);
        Task SaveChangesAsync();
    }
}
