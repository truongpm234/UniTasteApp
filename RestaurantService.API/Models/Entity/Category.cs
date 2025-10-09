using System;
using System.Collections.Generic;

namespace RestaurantService.API.Models.Entity;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? Name { get; set; }

    public string? SourceType { get; set; }
}
