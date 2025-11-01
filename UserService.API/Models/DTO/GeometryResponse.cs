namespace UserService.API.Models.DTO
{
    public class GeometryResponse
    {
        public LocationResponse Location { get; set; }
    }

    public class LocationResponse
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class OpeningHoursResponse
    {
        public List<string> WeekdayText { get; set; }
    }
}
