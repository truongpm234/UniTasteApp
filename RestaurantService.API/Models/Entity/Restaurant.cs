using System;
using System.Collections.Generic;

namespace RestaurantService.API.Models.Entity;

public partial class Restaurant
{
    public int RestaurantId { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? GooglePlaceId { get; set; }

    public string? Phone { get; set; }

    public string? Website { get; set; }

    public string? CoverImageUrl { get; set; }

    public double? GoogleRating { get; set; }

    public string? OpeningHours { get; set; }

    public int? PriceRangeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<BannerAd> BannerAds { get; set; } = new List<BannerAd>();

    public virtual ICollection<BannerBooking> BannerBookings { get; set; } = new List<BannerBooking>();

    //public virtual ICollection<OpeningHour> OpeningHoursNavigation { get; set; } = new List<OpeningHour>();

    public virtual PriceRange? PriceRange { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Feature> Features { get; set; } = new List<Feature>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

}
