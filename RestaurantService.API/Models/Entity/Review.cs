using System;
using System.Collections.Generic;

namespace RestaurantService.API.Models.Entity;

public partial class Review
{
    public int ReviewId { get; set; }

    public int RestaurantId { get; set; }

    public string UserName { get; set; } = null!;

    public double? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<PhotoReview> PhotoReviews { get; set; } = new List<PhotoReview>();

    public virtual Restaurant? Restaurant { get; set; }

}
