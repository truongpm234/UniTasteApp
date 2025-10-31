namespace PaymentService.API.Models.DTO
{
    public class CreatePaymentDto
    {
        public long OrderCode { get; set; }
        public long Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
