using System;
using System.Collections.Generic;

namespace UserService.API.Models.Entity;

public partial class Accessory
{
    public int AccessoryId { get; set; }

    public string? Name { get; set; }

    public string? ImageUrl { get; set; }

    public decimal? Price { get; set; }

    public string? Type { get; set; }

    public virtual ICollection<UserAccessory> UserAccessories { get; set; } = new List<UserAccessory>();
}
