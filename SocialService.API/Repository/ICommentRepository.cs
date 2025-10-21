using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public interface ICommentRepository
    {
        Task AddComment(Comment comment);
        Task SaveAsync();
    }
}
