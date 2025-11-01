namespace UserService.API.Models.DTO
{
    public class GooglePlaceResponse
    {
        public string PlaceId { get; set; }
        public string Name { get; set; }
        public string FormattedAddress { get; set; }
        public string Vicinity { get; set; }
        public double? Rating { get; set; }
        public int? PriceLevel { get; set; }
        public List<string> Types { get; set; }
        public GeometryResponse Geometry { get; set; }
        public OpeningHoursResponse OpeningHours { get; set; }
    }
}
