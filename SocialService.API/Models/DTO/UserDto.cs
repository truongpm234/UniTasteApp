namespace SocialService.API.Models.DTO
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? RoleName { get; set; }
        public string? Status { get; set; }

        // 👇 Thêm thuộc tính AvatarUrl để map đúng với dữ liệu trả về từ UserService
        public string? AvatarUrl { get; set; }

        // (Không bắt buộc nhưng có thể thêm cho tiện)
        public string? Bio { get; set; }
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
