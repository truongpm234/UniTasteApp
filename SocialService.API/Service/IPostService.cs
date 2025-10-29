using SocialService.API.Models.DTO;
using SocialService.API.Models.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialService.API.Service
{
    public interface IPostService
    {
        Task<Models.DTO.PagedResult<PostDto>> GetAllReviewsPagedAsync(int page, int pageSize);
        Task<List<Post>> GetAllPostByRestaurantId(int id);
        Task<IEnumerable<PostDto>> GetPostsByUserIdAsync(int userId);
        Task<(int postId, string? googlePlaceId)> CreatePostAsync(PostCreateDto dto, int userId);
        Task UpdatePostAsync(int postId, int userId, PostUpdateDto dto);
        Task DeletePostAsync(int postId, int userId);
        Task<List<Post>> GetAllPostOfRestaurantIdByUserId(int userId, int restaurantId);
        Task<List<DailyPostCountDto>> GetPostCountByDayLast14DaysAsync();

    }
}
