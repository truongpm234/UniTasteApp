using SocialService.API.Models.DTO;
using System.Linq.Dynamic.Core;
namespace SocialService.API.Service
{
    public interface IPostService
    {
        Task<Models.DTO.PagedResult<PostDto>> GetAllReviewsPagedAsync(int page, int pageSize);
        Task<int> CreatePostAsync(PostCreateDto dto, int userId);
        Task<IEnumerable<PostDto>> GetPostsByUserIdAsync(int userId);
        Task UpdatePostAsync(int postId, int userId, PostUpdateDto dto);
        Task DeletePostAsync(int postId, int userId);
    }
}
