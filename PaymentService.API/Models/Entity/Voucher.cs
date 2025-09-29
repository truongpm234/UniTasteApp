using System;
using System.Collections.Generic;

namespace PaymentService.API.Models.Entity;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? DiscountType { get; set; }

    public decimal? DiscountValue { get; set; }

    public int? RestaurantId { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<UserMiniGamePlay> UserMiniGamePlays { get; set; } = new List<UserMiniGamePlay>();

    public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();

    internal static async Task AddTransactionAsync(PaymentTransaction paymentTransaction)
    {
        throw new NotImplementedException();
    }
}
