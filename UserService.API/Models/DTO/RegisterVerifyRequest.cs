namespace UserService.API.Models.DTO
{
    public class RegisterVerifyRequest
    {
        public string Email { get; set; }
        public string OtpCode { get; set; }
    }
}
