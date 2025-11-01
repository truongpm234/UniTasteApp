namespace UserService.API.Models.DTO
{
    public class RestaurantDto
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double? GoogleRating { get; set; }
        public string GooglePlaceId { get; set; }
        public string OpeningHours { get; set; }
        public double Distance { get; set; }
    }
}
