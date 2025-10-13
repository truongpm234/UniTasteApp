namespace UserService.API.Models.DTO
{
    public class UpdateUserProfileForm
    {
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }

}
