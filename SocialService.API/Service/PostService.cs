using SocialService.API.Models.DTO;
using SocialService.API.Models.Entity;
using SocialService.API.Repository;

namespace SocialService.API.Service
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;
        private readonly IFirebaseStorageService _firebaseStorage;
        public PostService(IPostRepository repo, IFirebaseStorageService firebaseStorage)
        {
            _repo = repo;
            _firebaseStorage = firebaseStorage;
        }

        public async Task<IEnumerable<PostDto>> GetAllReviewsAsync()
        {
            var posts = await _repo.GetAllReviewsAsync();

            return posts.Select(p => new PostDto
            {
                PostId = p.PostId,
                AuthorUserId = p.AuthorUserId,
                Title = p.Title,
                Content = p.Content,
                Rating = p.Rating,
                Visibility = p.Visibility,
                CreatedAt = p.CreatedAt,
                MediaUrls = p.PostMedia.Select(m => m.MediaUrl).ToList(),
                Tags = p.Tags.Select(t => t.Name).ToList(),
                ReactionsCount = p.ReactionsCount,
                CommentsCount = p.CommentsCount,
                SharesCount = p.SharesCount
            });
        }

        public async Task<int> CreatePostAsync(PostCreateDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                throw new Exception("Nội dung bài viết không được để trống.");

            var post = new Post
            {
                AuthorUserId = userId,
                Title = dto.Title,
                Content = dto.Content,
                Rating = dto.Rating,
                IsReview = dto.IsReview,
                Visibility = dto.Visibility ?? "Public",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                ReactionsCount = 0,
                CommentsCount = 0,
                SharesCount = 0
            };

            await _repo.AddPostAsync(post);
            await _repo.SaveChangesAsync(); // cần có PostId trước khi thêm media

            // ✅ Upload ảnh
            if (dto.MediaFiles != null && dto.MediaFiles.Count > 0)
            {
                int sort = 1;
                foreach (var file in dto.MediaFiles)
                {
                    var url = await _firebaseStorage.UploadFileAsync(file, "posts");

                    var media = new PostMedium
                    {
                        PostId = post.PostId,
                        MediaUrl = url,
                        MediaType = file.ContentType,
                        SortOrder = sort++,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _repo.AddPostMediaAsync(media);
                }
                await _repo.SaveChangesAsync();
            }

            // ✅ Tag (nếu có)
            if (dto.Tags != null && dto.Tags.Count > 0)
            {
                foreach (var tagName in dto.Tags)
                {
                    var tag = await _repo.GetTagByNameAsync(tagName);
                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Name = tagName,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _repo.AddTagAsync(tag);
                        await _repo.SaveChangesAsync();
                    }

                    post.Tags.Add(tag);
                }
                await _repo.SaveChangesAsync();
            }

            return post.PostId;
        }
    }
}
