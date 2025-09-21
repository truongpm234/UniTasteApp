using Newtonsoft.Json;

namespace RestaurantService.API.Models.GooglePlaces
{
    public class GooglePlacesResponse
    {
        [JsonProperty("results")]
        public List<GooglePlace> Results { get; set; } = new List<GooglePlace>();

        [JsonProperty("next_page_token")]
        public string NextPageToken { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("reviews")]
        public List<GoogleReview> Reviews { get; set; }

    }

}
