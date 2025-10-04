namespace RestaurantService.API.Models.DTO
{
    public class LocationRequestDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusKm { get; set; } = 10;
    }

}
