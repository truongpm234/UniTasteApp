using Newtonsoft.Json;

namespace RestaurantService.API.Models.GooglePlaces
{
    public class GooglePhoto
    {
        [JsonProperty("photo_reference")]
        public string PhotoReference { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }
}
