using Newtonsoft.Json;

namespace RestaurantService.API.Models.GooglePlaces
{
    public class GooglePlace
    {
        [JsonProperty("place_id")]
        public string PlaceId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonProperty("geometry")]
        public GoogleGeometry Geometry { get; set; }

        [JsonProperty("rating")]
        public double? Rating { get; set; }

        [JsonProperty("vicinity")]
        public string Vicinity { get; set; }

        [JsonProperty("price_level")]
        public int? PriceLevel { get; set; }

        [JsonProperty("types")]
        public List<string> Types { get; set; } = new List<string>();

        [JsonProperty("photos")]
        public List<GooglePhoto> Photos { get; set; } = new List<GooglePhoto>();

        [JsonProperty("opening_hours")]
        public GoogleOpeningHours OpeningHours { get; set; }

        [JsonProperty("formatted_phone_number")]
        public string FormattedPhoneNumber { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }
        public string CoverImageUrl { get; set; }

        [JsonProperty("reviews")]
        public List<GoogleReview> Reviews { get; set; }
    }
}
