using SocialService.API.Data.DBContext;
using SocialService.API.Models.DTO;
using SocialService.API.Models.Entity;
using SocialService.API.Repository;

namespace SocialService.API.Service
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repo;
        private readonly Exe201SocialServiceDbContext _context;

        public CommentService(ICommentRepository repo, Exe201SocialServiceDbContext context)
        {
            _repo = repo;
            _context = context;
        }
        public async Task<List<Comment?>> GetCommentByPostIdAsync(int postId)
        {
            return await _repo.GetCommentByPostIdAsync(postId);
        }

        public async Task<int> CreateComment(CommentCreateDto dto, int userId)
        {
            var post = await _context.Posts.FindAsync(dto.PostId);
            if (post == null)
                throw new Exception("Post not found");

            var comment = new Comment
            {
                PostId = dto.PostId,
                AuthorUserId = userId,          // ✅ Lấy từ token
                Content = dto.Content,
                ParentId = dto.ParentId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                ReactionsCount = 0
            };

            await _repo.AddComment(comment);
            await _repo.SaveAsync();

            post.CommentsCount += 1;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return comment.CommentId;
        }
    }
}