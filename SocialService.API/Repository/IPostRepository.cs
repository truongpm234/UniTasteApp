using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public interface IPostRepository
    {
        Task<(IEnumerable<Post> posts, int totalCount)> GetAllReviewsPagedAsync(int page, int pageSize);
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId);
        Task<Post?> GetPostByIdAsync(int postId);
        Task DeletePostAsync(Post post);
        Task AddPostAsync(Post post);
        Task AddPostMediaAsync(PostMedium media);
        Task<Tag?> GetTagByNameAsync(string name);
        Task AddTagAsync(Tag tag);
        void UpdatePost(Post post);
        Task SaveChangesAsync();
    }
}
