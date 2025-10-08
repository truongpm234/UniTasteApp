using RestaurantService.API.Models.Entity;

namespace RestaurantService.API.Models.DTO
{
    public class RestaurantResponseDto
    {
        public int RestaurantId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? GooglePlaceId { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? CoverImageUrl { get; set; }
        public double? GoogleRating { get; set; }
        public int? PriceRangeId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
        public PriceRange? PriceRange { get; set; }
        public List<Feature>? Features { get; set; }
        public List<Review>? Reviews { get; set; }
        public List<Category>? Categories { get; set; }
        public List<OpeningHourDto2>? OpeningHours { get; set; }
    }

}
