using System;
using System.Collections.Generic;

namespace PaymentService.API.Models.Entity;

public partial class UserWallet
{
    public int UserId { get; set; }

    public decimal? Balance { get; set; }
}
