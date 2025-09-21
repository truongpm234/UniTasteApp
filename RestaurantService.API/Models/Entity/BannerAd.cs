using System;
using System.Collections.Generic;

namespace RestaurantService.API.Models.Entity;

public partial class BannerAd
{
    public int BannerAdId { get; set; }

    public int? RestaurantId { get; set; }

    public string? ImageUrl { get; set; }

    public string? LinkUrl { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }
}
