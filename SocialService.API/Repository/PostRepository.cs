using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<(IEnumerable<Post> posts, int totalCount)> GetAllReviewsPagedAsync(int page, int pageSize)
        {
            var query = _context.Posts
                .Where(p => !p.IsDeleted && p.IsReview)
                .Include(p => p.PostMedia)
                .Include(p => p.Tags)
                .Include(p => p.PostReactions)
                .Include(p => p.Comments)
                .Include(p => p.PostRestaurantTags)
                .OrderByDescending(p => p.CreatedAt);

            int totalCount = await query.CountAsync();

            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (posts, totalCount);
        }
        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(int userId)
        {
            return await _context.Posts
                .Where(p => p.AuthorUserId == userId && !p.IsDeleted)
                .Include(p => p.PostMedia)
                .Include(p => p.Tags)
                .Include(p => p.PostReactions)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post?> GetPostByIdAsync(int postId)
        {
            return await _context.Posts
                .Include(p => p.PostMedia)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);
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

        public void UpdatePost(Post post)
        {
            _context.Posts.Update(post);
        }

        public async Task DeletePostAsync(Post post)
        {
            post.IsDeleted = true;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task AddPostRestaurantTagAsync(PostRestaurantTag tag)
        {
            await _context.PostRestaurantTags.AddAsync(tag);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
