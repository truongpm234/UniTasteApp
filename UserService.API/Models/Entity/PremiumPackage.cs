using System;
using System.Collections.Generic;

namespace UserService.API.Models.Entity;

public partial class PremiumPackage
{
    public int PremiumPackageId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? DurationDays { get; set; }

    public virtual ICollection<UserPremium> UserPremia { get; set; } = new List<UserPremium>();
}
