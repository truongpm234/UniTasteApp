using System;
using System.Collections.Generic;

namespace RestaurantService.API.Models.Entity;

public partial class PhotoReview
{
    public int PhotoReviewId { get; set; }

    public int ReviewId { get; set; }

    public string PhotoUrl { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Review Review { get; set; } = null!;
}
