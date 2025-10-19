using System;
using System.Collections.Generic;

namespace PaymentService.API.Models.Entity;

public partial class ServicePackage
{
    public int ServicePackageId { get; set; } 
    public string Name { get; set; } = string.Empty; 
    public int DurationInMonths { get; set; }       
    public decimal Price { get; set; }             
    public string? Description { get; set; }       
    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
