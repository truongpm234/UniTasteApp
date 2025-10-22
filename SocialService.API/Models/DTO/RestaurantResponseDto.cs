namespace SocialService.API.Models.DTO
{
    public class RestaurantResponseDto
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; } = null!;
        public string? GooglePlaceId { get; set; }
    }
}
