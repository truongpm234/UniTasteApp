using RestaurantService.API.Models.GooglePlaces;

namespace RestaurantService.API.Models.DTO
{
    public class GooglePlaceDTO
    {
        public string PlaceId { get; set; }
        public string Name { get; set; }
        public string FormattedAddress { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Website { get; set; }
        public string Phone { get; set; }
        public string PriceLevel { get; set; }
        public double? Rating { get; set; }
        public string OpeningHours { get; set; }
        public List<string> Types { get; set; }
        public string CoverImageUrl { get; set; }
        public List<GoogleReview> Reviews { get; set; }
        public int? PriceRangeId { get; set; }
        public List<int> CategoryIds { get; set; }
    }
}
