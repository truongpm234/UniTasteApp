namespace UserService.API.Models.DTO
{
    public class ConfirmResetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

}
