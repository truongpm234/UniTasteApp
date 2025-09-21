using System;
using System.Collections.Generic;

namespace PaymentService.API.Models.Entity;

public partial class PaymentTransaction
{
    public int PaymentTransactionId { get; set; }

    public int? UserId { get; set; }

    public decimal? Amount { get; set; }

    public string? TransactionType { get; set; }

    public string? Status { get; set; }

    public string? ReferenceId { get; set; }

    public DateTime? CreatedAt { get; set; }
}
