using Microsoft.EntityFrameworkCore;
using SocialService.API.Data.DBContext;
using SocialService.API.Models.DTO;
using SocialService.API.Models.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<List<Post>> GetAllPostByRestaurantId(int id)
        {
            return await _context.PostRestaurantTags
                .Where(prt => prt.RestaurantId == id)
                .Include(prt => prt.Post)
                    .ThenInclude(p => p.PostMedia)
                .Include(prt => prt.Post)
                    .ThenInclude(p => p.Tags)
                .Include(prt => prt.Post)
                    .ThenInclude(p => p.PostReactions)
                .Include(prt => prt.Post)
                    .ThenInclude(p => p.Comments)
                .Select(prt => prt.Post)
                .Where(p => !p.IsDeleted)
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

        public async Task<List<Post>> GetAllPostOfRestaurantIdByUserId(int userId, int restaurantId)
        {
            return await _context.PostRestaurantTags
                .Where(prt => prt.RestaurantId == restaurantId && prt.Post.AuthorUserId == userId)
                .Include(prt => prt.Post)
                    .ThenInclude(p => p.PostMedia)
                .Include(prt => prt.Post)
                    .ThenInclude(p => p.Tags)
                .Include(prt => prt.Post)
                    .ThenInclude(p => p.PostReactions)
                .Include(prt => prt.Post)
                    .ThenInclude(p => p.Comments)
                .Select(prt => prt.Post)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<DailyPostCountDto>> GetPostCountByDayLast14DaysAsync()
        {
            var fromDate = DateTime.UtcNow.Date.AddDays(-13); 
            var toDate = DateTime.UtcNow.Date.AddDays(1);   

            var data = await _context.Posts
                .Where(p =>
                    !p.IsDeleted &&
                    p.CreatedAt >= fromDate && p.CreatedAt < toDate)
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new DailyPostCountDto
                {
                    Date = g.Key,
                    PostCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Đảm bảo trả đủ 14 ngày
            var results = new List<DailyPostCountDto>();
            for (int i = 0; i < 14; i++)
            {
                var d = fromDate.AddDays(i);
                var found = data.FirstOrDefault(x => x.Date.Date == d.Date);
                results.Add(new DailyPostCountDto
                {
                    Date = d,
                    PostCount = found?.PostCount ?? 0
                });
            }
            return results;
        }

    }
}
