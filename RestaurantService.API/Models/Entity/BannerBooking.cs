using System;
using System.Collections.Generic;

namespace RestaurantService.API.Models.Entity;

public partial class BannerBooking
{
    public int BannerBookingId { get; set; }

    public int? RestaurantId { get; set; }

    public int? UserId { get; set; }

    public int? BannerAdId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? BookingDate { get; set; }

    public string? Status { get; set; }
}
