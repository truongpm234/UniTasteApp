namespace SocialService.API.Models.DTO
{
    public class CommentCreateDto
    {
        public int PostId { get; set; }              // ID bài viết
        public string Content { get; set; } = null!; // Nội dung bình luận
        public int? ParentId { get; set; }
    }
}
