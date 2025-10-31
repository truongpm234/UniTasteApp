using System;
using System.Collections.Generic;

namespace PaymentService.API.Models.DTO
{
    public class PaymentRequest
    {
        public long orderCode { get; set; }
        public long amount { get; set; }
        public string? description { get; set; }
        public string? returnUrl { get; set; }
        public string? cancelUrl { get; set; }
        public string? signature { get; set; }
    }
}


