namespace RestaurantService.API.Models.DTO
{
    public class LocationCategoryRequestDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusKm { get; set; }
        public string CategoryName { get; set; }
    }

}
