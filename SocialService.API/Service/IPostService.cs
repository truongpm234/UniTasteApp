using SocialService.API.Models.DTO;

namespace SocialService.API.Service
{
    public interface IPostService
    {
        Task<IEnumerable<PostDto>> GetAllReviewsAsync();
        Task<int> CreatePostAsync(PostCreateDto dto, int userId);
    }
}
