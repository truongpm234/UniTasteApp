using System.Collections.Generic;
using System.Threading.Tasks;
using SocialService.API.Models.DTO;

namespace SocialService.API.Service
{
    public interface IPostService
    {
        Task<Models.DTO.PagedResult<PostDto>> GetAllReviewsPagedAsync(int page, int pageSize);
        Task<IEnumerable<PostDto>> GetPostsByUserIdAsync(int userId);
        Task<(int postId, string? googlePlaceId)> CreatePostAsync(PostCreateDto dto, int userId);
        Task UpdatePostAsync(int postId, int userId, PostUpdateDto dto);
        Task DeletePostAsync(int postId, int userId);
    }
}
