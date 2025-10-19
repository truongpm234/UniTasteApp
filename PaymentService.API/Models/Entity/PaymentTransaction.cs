using System;
using System.Collections.Generic;

namespace PaymentService.API.Models.Entity;

public partial class PaymentTransaction
{
    public int PaymentTransactionId { get; set; }

    public int? UserId { get; set; }

    public decimal? Amount { get; set; }

    public string? TransactionType { get; set; }= string.Empty; 

    public string? Status { get; set; } = string.Empty; 

    public string? ReferenceId { get; set; } = string.Empty;        

    public DateTime? CreatedAt { get; set; }

    public long OrderCode { get; internal set; }

    public string Description { get; internal set; }
}


