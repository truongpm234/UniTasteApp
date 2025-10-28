using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SocialService.API.Data.DBContext;
using SocialService.API.Models.DTO;
using SocialService.API.Models.Entity;
using SocialService.API.Repository;

namespace SocialService.API.Service
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;
        private readonly IFirebaseStorageService _firebaseStorage;
        private readonly Exe201SocialServiceDbContext _context;
        private readonly IRestaurantApiService _restaurantApi;
        private readonly ICloudinaryService _cloudinaryService;
        public PostService(
            IPostRepository repo,
            IFirebaseStorageService firebaseStorage,
            Exe201SocialServiceDbContext context,
            IRestaurantApiService restaurantApi,
            ICloudinaryService cloudinaryService)
        {
            _repo = repo;
            _firebaseStorage = firebaseStorage;
            _context = context;
            _restaurantApi = restaurantApi;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Models.DTO.PagedResult<PostDto>> GetAllReviewsPagedAsync(int page, int pageSize)
        {
            var (posts, totalCount) = await _repo.GetAllReviewsPagedAsync(page, pageSize);

            var data = posts.Select(p => new PostDto
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
            }).ToList();

            return new Models.DTO.PagedResult<PostDto>
            {
                Items = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        public async Task<IEnumerable<PostDto>> GetPostsByUserIdAsync(int userId)
        {
            var posts = await _repo.GetPostsByUserIdAsync(userId);

            return posts.Select(p => new PostDto
            {
                PostId = p.PostId,
                AuthorUserId = p.AuthorUserId,
                Title = p.Title,
                Content = p.Content,
                Rating = (byte?)p.Rating,
                IsReview = p.IsReview,
                Visibility = p.Visibility,
                CreatedAt = p.CreatedAt,
                MediaUrls = p.PostMedia.Select(m => m.MediaUrl).ToList(),
                Tags = p.Tags.Select(t => t.Name).ToList(),
                ReactionsCount = p.ReactionsCount,
                CommentsCount = p.CommentsCount,
                SharesCount = p.SharesCount
            });
        }

        public async Task DeletePostAsync(int postId, int userId)
        {
            var post = await _repo.GetPostByIdAsync(postId);
            if (post == null) throw new Exception("Bài viết không tồn tại.");

            if (post.AuthorUserId != userId)
                throw new Exception("Bạn không có quyền xóa bài viết này.");

            post.IsDeleted = true;
            post.UpdatedAt = DateTime.UtcNow;
            await _repo.DeletePostAsync(post);
        }


        public async Task<(int postId, string? googlePlaceId)> CreatePostAsync(PostCreateDto dto, int userId)
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
            await _repo.SaveChangesAsync(); // Lấy PostId

            // Upload nhiều ảnh lên Cloudinary
            if (dto.MediaFiles != null && dto.MediaFiles.Count > 0)
            {
                int sort = 1;
                var urls = await _cloudinaryService.UploadImagesAsync(dto.MediaFiles, "posts");
                foreach (var url in urls)
                {
                    var file = dto.MediaFiles[sort - 1]; // Để lấy ContentType
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

            // Gắn Tag (nếu có)
            if (dto.Tags != null && dto.Tags.Count > 0)
            {
                foreach (var tagName in dto.Tags)
                {
                    var tag = await _repo.GetTagByNameAsync(tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName, CreatedAt = DateTime.UtcNow };
                        await _repo.AddTagAsync(tag);
                        await _repo.SaveChangesAsync();
                    }
                    post.Tags.Add(tag);
                }
                await _repo.SaveChangesAsync();
            }

            // Gắn RestaurantTag (nếu có)
            string? googlePlaceId = null;
            if (dto.RestaurantId.HasValue)
            {
                googlePlaceId = await _restaurantApi.GetGooglePlaceIdAsync(dto.RestaurantId.Value);
                if (string.IsNullOrEmpty(googlePlaceId))
                    throw new Exception("Không tìm thấy GooglePlaceId từ RestaurantService.");

                var restaurantTag = new PostRestaurantTag
                {
                    PostId = post.PostId,
                    RestaurantId = dto.RestaurantId.Value,
                    GooglePlaceId = googlePlaceId,
                    CreatedAt = DateTime.UtcNow
                };

                await _repo.AddPostRestaurantTagAsync(restaurantTag);
                await _repo.SaveChangesAsync();
            }

            return (post.PostId, googlePlaceId);
        }

        public async Task UpdatePostAsync(int postId, int userId, PostUpdateDto dto)
        {
            var post = await _repo.GetPostByIdAsync(postId);
            if (post == null) throw new Exception("Bài viết không tồn tại.");
            if (post.AuthorUserId != userId) throw new Exception("Bạn không có quyền chỉnh sửa bài viết này.");

            if (!string.IsNullOrWhiteSpace(dto.Title)) post.Title = dto.Title;
            if (!string.IsNullOrWhiteSpace(dto.Content)) post.Content = dto.Content;
            if (dto.Rating.HasValue) post.Rating = dto.Rating;
            if (dto.IsReview.HasValue) post.IsReview = dto.IsReview.Value;
            if (!string.IsNullOrWhiteSpace(dto.Visibility)) post.Visibility = dto.Visibility;
            post.UpdatedAt = DateTime.UtcNow;

            if (dto.MediaFiles != null && dto.MediaFiles.Count > 0)
            {
                _context.PostMedia.RemoveRange(post.PostMedia);
                int sort = 1;
                var urls = await _cloudinaryService.UploadImagesAsync(dto.MediaFiles, "posts");
                foreach (var url in urls)
                {
                    var file = dto.MediaFiles[sort - 1];
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
            }

            if (dto.Tags != null && dto.Tags.Count > 0)
            {
                post.Tags.Clear();
                foreach (var tagName in dto.Tags)
                {
                    var tag = await _repo.GetTagByNameAsync(tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName, CreatedAt = DateTime.UtcNow };
                        await _repo.AddTagAsync(tag);
                        await _repo.SaveChangesAsync();
                    }
                    post.Tags.Add(tag);
                }
            }

            _repo.UpdatePost(post);
            await _repo.SaveChangesAsync();
        }

        public async Task<List<Post>> GetAllPostByRestaurantId(int id)
        {
            return await _repo.GetAllPostByRestaurantId(id);
        }

        public async Task<List<Post>> GetAllPostOfRestaurantIdByUserId(int userId, int restaurantId)
        {
            return await _repo.GetAllPostOfRestaurantIdByUserId(userId, restaurantId);
        }
    }
}
