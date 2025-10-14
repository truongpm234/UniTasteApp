using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Ocelot.Middleware;
using Ocelot.Multiplexer;
using Ocelot.Headers;
using System.Net.Http;
using System.Text;

namespace ApiGateway.Aggregators
{
    public class UserPaymentAggregator : IDefinedAggregator
    {
        public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
        {
            if (responses.Count < 2)
            {
                var error = JsonConvert.SerializeObject(new { message = "Missing downstream responses" });
                return new DownstreamResponse(
                    new StringContent(error, Encoding.UTF8, "application/json"),
                    System.Net.HttpStatusCode.BadGateway,
                    new List<Header>(),
                    "Error"
                );
            }

            try
            {
                // Lấy dữ liệu từ 2 service
                var userResponse = await responses[0].Items.DownstreamResponse().Content.ReadAsStringAsync();
                var paymentResponse = await responses[1].Items.DownstreamResponse().Content.ReadAsStringAsync();

                var users = JsonConvert.DeserializeObject<List<UserDto>>(userResponse) ?? new();
                var payments = JsonConvert.DeserializeObject<List<PaymentTransactionDto>>(paymentResponse) ?? new();

                // Gộp dữ liệu
                foreach (var user in users)
                {
                    user.PaymentTransactions = payments
                        .Where(p => p.UserId == user.UserId)
                        .ToList();
                }

                var mergedJson = JsonConvert.SerializeObject(users, Formatting.Indented);
                var stringContent = new StringContent(mergedJson, Encoding.UTF8, "application/json");

                return new DownstreamResponse(
                    stringContent,
                    System.Net.HttpStatusCode.OK,
                    new List<Header>(),
                    "OK"
                );
            }
            catch (Exception ex)
            {
                var error = JsonConvert.SerializeObject(new { message = ex.Message });
                return new DownstreamResponse(
                    new StringContent(error, Encoding.UTF8, "application/json"),
                    System.Net.HttpStatusCode.InternalServerError,
                    new List<Header>(),
                    "Error"
                );
            }
        }
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? PhoneNumber { get; set; }

        public List<PaymentTransactionDto> PaymentTransactions { get; set; } = new();
    }

    public class PaymentTransactionDto
    {
        public int PaymentTransactionId { get; set; }
        public int? UserId { get; set; }
        public decimal? Amount { get; set; }
        public string? TransactionType { get; set; }
        public string? Status { get; set; }
        public string? ReferenceId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public long OrderCode { get; set; }
        public string? Description { get; set; }
    }
}
