using System;
using System.Collections.Generic;

namespace PaymentService.API.Models.Entity;

public partial class UserMiniGamePlay
{
    public int UserMiniGamePlayId { get; set; }

    public int? MiniGameId { get; set; }

    public int? UserId { get; set; }

    public DateTime? PlayDate { get; set; }

    public string? Result { get; set; }

    public int? RewardVoucherId { get; set; }

    public virtual Voucher? RewardVoucher { get; set; }
}
