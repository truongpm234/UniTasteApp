namespace SocialService.API.Models.DTO
{
    public class PostUpdateDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public byte? Rating { get; set; }
        public bool? IsReview { get; set; }
        public string? Visibility { get; set; }
        public List<IFormFile>? MediaFiles { get; set; }
        public List<string>? Tags { get; set; }
    }
}
