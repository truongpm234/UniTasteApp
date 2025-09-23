namespace UserService.API.Models.DTO
{
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateOnly BirthDate { get; set; }
    }

}
