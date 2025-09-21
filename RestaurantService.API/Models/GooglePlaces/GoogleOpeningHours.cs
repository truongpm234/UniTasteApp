using Newtonsoft.Json;

namespace RestaurantService.API.Models.GooglePlaces
{
    public class GoogleOpeningHours
    {
        [JsonProperty("open_now")]
        public bool? OpenNow { get; set; }

        [JsonProperty("weekday_text")]
        public List<string> WeekdayText { get; set; } = new List<string>();
    }
}
