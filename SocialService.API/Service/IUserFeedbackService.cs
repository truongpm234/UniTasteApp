using SocialService.API.Models.Entity;

namespace SocialService.API.Service
{
    public interface IUserFeedbackService
    {
        Task<UserFeedback> AddFeedbackAsync(UserFeedback feedback);
        Task<List<UserFeedback>> GetAllAsync();
        Task<double> GetAverageRatingAsync();
    }
}