namespace SocialService.API.Models.DTO
{
    public class PostCreateDto
    {
        public string? Title { get; set; }
        public string Content { get; set; } = null!;
        public byte? Rating { get; set; }
        public bool IsReview { get; set; } = true;
        public string Visibility { get; set; } = "Public";
        public List<IFormFile>? MediaFiles { get; set; }   
        public List<string>? Tags { get; set; }            
    }
}
