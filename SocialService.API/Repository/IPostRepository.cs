using System.Collections.Generic;
using System.Threading.Tasks;
using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllReviewsAsync();
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId);
        Task<Post?> GetPostByIdAsync(int postId);
        Task AddPostAsync(Post post);
        Task AddPostMediaAsync(PostMedium media);
        Task<Tag?> GetTagByNameAsync(string name);
        Task AddTagAsync(Tag tag);
        void UpdatePost(Post post);
        Task DeletePostAsync(Post post);
        Task SaveChangesAsync();
        Task AddPostRestaurantTagAsync(PostRestaurantTag tag);
    }
}
