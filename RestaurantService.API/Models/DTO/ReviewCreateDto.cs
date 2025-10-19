namespace RestaurantService.API.Models.DTO
{
    public class ReviewCreateDto
    {
        public int RestaurantId { get; set; }
        public string UserName { get; set; } = null!;
        public double? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UserId { get; set; }
    }

}
