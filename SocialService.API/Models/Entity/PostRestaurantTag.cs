using System;
using System.Collections.Generic;

namespace SocialService.API.Models.Entity;

public partial class PostRestaurantTag
{
    public int PostRestaurantTagId { get; set; }

    public int PostId { get; set; }

    public int RestaurantId { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? GooglePlaceId { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
