using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllReviewsAsync();
        Task AddPostAsync(Post post);
        Task AddPostMediaAsync(PostMedium media);
        Task<Tag?> GetTagByNameAsync(string name);
        Task AddTagAsync(Tag tag);
        Task SaveChangesAsync();
    }
}
