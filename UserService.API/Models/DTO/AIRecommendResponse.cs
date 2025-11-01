namespace UserService.API.Models.DTO
{
    public class AIRecommendResponse
    {
        public string Message { get; set; }
        public List<string> RecommendedPlaceIds { get; set; }
    }
}
