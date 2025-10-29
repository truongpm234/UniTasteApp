using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public interface ICommentRepository
    {
        Task<List<Comment?>> GetCommentByPostIdAsync(int postId);
        Task AddComment(Comment comment);
        Task SaveAsync();
    }
}
