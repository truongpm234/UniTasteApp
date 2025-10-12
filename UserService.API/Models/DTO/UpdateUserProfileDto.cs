namespace UserService.API.Models.DTO
{
    public class UpdateUserProfileDto
    {
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }
    }

}
