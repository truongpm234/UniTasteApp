using SocialService.API.Models.DTO;
using SocialService.API.Models.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialService.API.Repository
{
    public interface IPostRepository
    {
        Task<(IEnumerable<Post> posts, int totalCount)> GetAllReviewsPagedAsync(int page, int pageSize);
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId);
        Task<List<Post>> GetAllPostByRestaurantId(int id);
        Task<Post?> GetPostByIdAsync(int postId);
        Task AddPostAsync(Post post);
        Task AddPostMediaAsync(PostMedium media);
        Task<Tag?> GetTagByNameAsync(string name);
        Task AddTagAsync(Tag tag);
        void UpdatePost(Post post);
        Task DeletePostAsync(Post post);
        Task SaveChangesAsync();
        Task AddPostRestaurantTagAsync(PostRestaurantTag tag);
        Task<List<Post>> GetAllPostOfRestaurantIdByUserId(int userId, int restaurantId);
        Task<List<DailyPostCountDto>> GetPostCountByDayLast14DaysAsync();

    }
}
