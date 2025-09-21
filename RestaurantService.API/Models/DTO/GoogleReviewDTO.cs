namespace RestaurantService.API.Models.DTO
{
    public class GoogleReviewDTO
    {
        public string AuthorName { get; set; }
        public string AuthorUrl { get; set; }
        public string ProfilePhotoUrl { get; set; }
        public double? Rating { get; set; }
        public string Text { get; set; }
        public long Time { get; set; }
    }
}
