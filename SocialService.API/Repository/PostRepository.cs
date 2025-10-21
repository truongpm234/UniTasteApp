using Microsoft.EntityFrameworkCore;
using SocialService.API.Data.DBContext;
using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly Exe201SocialServiceDbContext _context;
        public PostRepository(Exe201SocialServiceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Post>> GetAllReviewsAsync()
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted && p.IsReview)
                .Include(p => p.PostMedia)          // ảnh đính kèm
                .Include(p => p.Tags)               // hashtag nhiều-nhiều
                .Include(p => p.PostReactions)      // cảm xúc
                .Include(p => p.Comments)           // bình luận
                .Include(p => p.PostRestaurantTags) // vị trí quán
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task AddPostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
        }

        public async Task AddPostMediaAsync(PostMedium media)
        {
            await _context.PostMedia.AddAsync(media);
        }

        public async Task<Tag?> GetTagByNameAsync(string name)
        {
            return await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task AddTagAsync(Tag tag)
        {
            await _context.Tags.AddAsync(tag);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
