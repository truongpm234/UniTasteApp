namespace PaymentService.API.Models.DTO
{
    // DTO trả về cho chart
    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
