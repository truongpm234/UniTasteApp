namespace SocialService.API.Models.DTO
{
    public class PostDto
    {
        public int PostId { get; set; }
        public int AuthorUserId { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = null!;
        public double? Rating { get; set; }
        public bool IsReview { get; set; }
        public string Visibility { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public List<string> MediaUrls { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public int ReactionsCount { get; set; }
        public int CommentsCount { get; set; }
        public int SharesCount { get; set; }
    }
}
