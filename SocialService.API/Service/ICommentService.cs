using SocialService.API.Models.DTO;
using SocialService.API.Models.Entity;

namespace SocialService.API.Service
{
    public interface ICommentService
    {
        Task<List<Comment?>> GetCommentByPostIdAsync(int postId);
        Task<int> CreateComment(CommentCreateDto dto, int userId);
    }
}
