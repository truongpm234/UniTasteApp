namespace SocialService.API.Models.DTO
{
    public class PostShareCreateDto
    {
        public int OriginalPostId { get; set; }
        public string? ShareComment { get; set; }
    }
}
