namespace UserService.API.Models.DTO
{
    public class CreateUserPreferenceDto
    {
        public int UserId { get; set; }
        public string? PreferredPlaceTypes { get; set; }
        public string? PreferredPriceRange { get; set; }
        public string? PreferredLocation { get; set; }
        public string? GoingWith { get; set; }
        public string? Purpose { get; set; }
        public string? RequiredFeatures { get; set; }
        public string? Note { get; set; }
        public string? VenueAtmosphere { get; set; }
        public string? CuisineType { get; set; }
        public string? VisitTime { get; set; }
    }
}
