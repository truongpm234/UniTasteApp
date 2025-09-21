using System;
using System.Collections.Generic;

namespace PaymentService.API.Models.Entity;

public partial class UserVoucher
{
    public int UserVoucherId { get; set; }

    public int? UserId { get; set; }

    public int? VoucherId { get; set; }

    public DateTime? AssignedDate { get; set; }

    public bool? IsUsed { get; set; }

    public DateTime? UsedDate { get; set; }

    public int? MiniGamePlayId { get; set; }

    public virtual Voucher? Voucher { get; set; }
}
