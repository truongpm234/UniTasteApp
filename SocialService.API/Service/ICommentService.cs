using SocialService.API.Models.DTO;

namespace SocialService.API.Service
{
    public interface ICommentService
    {
        Task<int> CreateComment(CommentCreateDto dto, int userId);
    }
}
