using System;

namespace UserService.API.Models.Entity
{
    public partial class UserPreference
    {
        public int UserPreferenceId { get; set; }
        public int UserId { get; set; }
        public string? PreferredPlaceTypes { get; set; }   
        public string? PreferredPriceRange { get; set; }   
        public string? PreferredLocation { get; set; }    
        public string? GoingWith { get; set; }            
        public string? Purpose { get; set; }               
        public string? RequiredFeatures { get; set; }    
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? VenueAtmosphere { get; set; }   
        public string? CuisineType { get; set; }       
        public string? VisitTime { get; set; }
        public virtual User? User { get; set; }
    }
}
