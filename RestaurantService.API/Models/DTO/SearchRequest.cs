namespace RestaurantService.API.Models.DTO
{
    public class SearchRequest
    {
        public int? currentPage { get; set; }
        public int? pageSize { get; set; }
    }

    public class RestaurantSearchRequest : SearchRequest
    {
        public string name { get; set; }
        public int quantity { get; set; }
        public string description { get; set; }
    }
}
