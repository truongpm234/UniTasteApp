namespace UserService.API.Models.DTO
{
    public class GooglePlaceSimpleDto
    {
        public string PlaceId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Rating { get; set; }
        public int? PriceLevel { get; set; }
        public string OpeningHours { get; set; }
        public List<string> Types { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
