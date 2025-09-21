using System;
using System.Collections.Generic;

namespace RestaurantService.API.Models.Entity;

public partial class Feature
{
    public int FeatureId { get; set; }

    public string? Name { get; set; }
}
