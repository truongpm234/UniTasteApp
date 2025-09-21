using System;
using System.Collections.Generic;

namespace UserService.API.Models.Entity;

public partial class UserPremium
{
    public int UserPremiumId { get; set; }

    public int? UserId { get; set; }

    public int? PremiumPackageId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual PremiumPackage? PremiumPackage { get; set; }

    public virtual User? User { get; set; }
}
