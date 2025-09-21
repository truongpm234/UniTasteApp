using System;
using System.Collections.Generic;

namespace UserService.API.Models.Entity;

public partial class UserAccessory
{
    public int UserAccessoryId { get; set; }

    public int? UserId { get; set; }

    public int? AccessoryId { get; set; }

    public DateTime? AcquiredAt { get; set; }

    public bool? IsEquipped { get; set; }

    public virtual Accessory? Accessory { get; set; }

    public virtual User? User { get; set; }
}
