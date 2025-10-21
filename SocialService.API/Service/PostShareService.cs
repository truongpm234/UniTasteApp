using SocialService.API.Data.DBContext;
using SocialService.API.Models.DTO;
using SocialService.API.Models.Entity;
using SocialService.API.Repository;

namespace SocialService.API.Service
{
    public class PostShareService : IPostShareService
    {
        private readonly Exe201SocialServiceDbContext _context;
        private readonly IPostShareRepository _repo;

        public PostShareService(Exe201SocialServiceDbContext context, IPostShareRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        public async Task<int> SharePostAsync(PostShareCreateDto dto, int userId)
        {
            var post = await _context.Posts.FindAsync(dto.OriginalPostId);
            if (post == null) throw new Exception("Post not found");

            var alreadyShared = _context.PostShares
                .Any(s => s.OriginalPostId == dto.OriginalPostId && s.SharerUserId == userId);
            if (alreadyShared)
                throw new Exception("You have already shared this post.");

            var share = new PostShare
            {
                OriginalPostId = dto.OriginalPostId,
                SharerUserId = userId,
                ShareComment = dto.ShareComment,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddShareAsync(share);
            await _repo.SaveAsync();

            post.SharesCount += 1;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return share.PostShareId;
        }
    }
}
