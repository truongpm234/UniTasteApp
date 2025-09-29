using System;
using System.Collections.Generic;

namespace PaymentService.API.Models.DTO
{
    public class PaymentResponse
    {
        public string code { get; set; }
        public string desc { get; set; }
        public PaymentResponseData? data { get; set; }
    }
    public class PaymentResponseData
    {
        public string? checkoutUrl { get; set; }
    
    }
}