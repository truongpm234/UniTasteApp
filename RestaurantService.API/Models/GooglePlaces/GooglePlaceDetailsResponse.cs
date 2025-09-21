using Newtonsoft.Json;

namespace RestaurantService.API.Models.GooglePlaces
{
    public class GooglePlaceDetailsResponse
    {
        [JsonProperty("result")]
        public GooglePlace Result { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }


}
