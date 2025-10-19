using System;
using System.Collections.Generic;

namespace PaymentService.API.Models.Entity;

public partial class Purchase
{
    public int PurchaseId { get; set; }

    public int? UserId { get; set; }

    public string? PurchaseType { get; set; }

    public decimal? Amount { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ServicePackageId { get; set; }

    public virtual ServicePackage? ServicePackage { get; set; }
}
