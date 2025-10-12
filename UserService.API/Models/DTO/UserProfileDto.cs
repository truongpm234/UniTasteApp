namespace UserService.API.Models.DTO
{
    public class UserProfileDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }
    }

}
