using Newtonsoft.Json;

namespace RestaurantService.API.Models.GooglePlaces
{
    public class GoogleGeometry
    {
        [JsonProperty("location")]
        public GoogleLocation Location { get; set; }
    }
}
