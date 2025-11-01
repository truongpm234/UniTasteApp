namespace UserService.API.Models.DTO
{
    public class ReviewGroupDto
    {
        public int RestaurantId { get; set; }
        public List<ReviewDto> Reviews { get; set; }
    }
}
