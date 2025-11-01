namespace SocialService.API.Models.Entity
{
    public partial class UserFeedback
    {
        public int UserFeedbackId { get; set; }
        public int? UserId { get; set; }
        public double Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
