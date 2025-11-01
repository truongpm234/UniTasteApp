using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public interface IUserFeedbackRepository
    {
        Task<UserFeedback> AddAsync(UserFeedback feedback);
        Task<List<UserFeedback>> GetAllAsync();
        Task<double> GetAverageRatingAsync();
    }
}