using RestaurantService.API.Models.Entity;

namespace RestaurantService.API.Models.DTO
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public string? SourceType { get; set; }
    }

    

}
