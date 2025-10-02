namespace UserService.API.Models.DTO
{
    public class RegisterVerification
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PasswordHash { get; set; }
        public DateTime BirthDate { get; set; }
        public string OtpCode { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

}
