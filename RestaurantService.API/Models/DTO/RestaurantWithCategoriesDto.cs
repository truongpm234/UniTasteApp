using RestaurantService.API.Models.Entity;

namespace RestaurantService.API.Models.DTO
{
    public class RestaurantWithCategoriesDto
    {
        public int RestaurantId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? CoverImageUrl { get; set; }
        public double? GoogleRating { get; set; }
        public List<CategoryDto> Categories { get; set; } = new();
    }

}
