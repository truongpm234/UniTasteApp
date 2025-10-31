namespace PaymentService.API.Models.DTO
{
    public class ManualCreatePaymentDto
    {
        public int ServicePackageId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }

}
