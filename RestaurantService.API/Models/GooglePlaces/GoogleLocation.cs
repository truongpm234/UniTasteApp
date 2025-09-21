using Newtonsoft.Json;

namespace RestaurantService.API.Models.GooglePlaces
{
    public class GoogleLocation
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lng { get; set; }
    }
}
