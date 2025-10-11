using Newtonsoft.Json;

namespace RestaurantService.API.Models.GooglePlaces
{
    public class GooglePlacesSearchRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Radius { get; set; } = 5000; // 5km
        public string Type { get; set; } 
        public string Keyword { get; set; }
        public string NextPageToken { get; set; }
        
    }

}
