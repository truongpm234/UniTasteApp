using System;
using System.Collections.Generic;

namespace RestaurantService.API.Models.Entity;

public partial class PriceRange
{
    public int PriceRangeId { get; set; }

    public string? Name { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }
}
