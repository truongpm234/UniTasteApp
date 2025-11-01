using SocialService.API.Models.Entity;
using SocialService.API.Repository;

namespace SocialService.API.Service
{
    public class UserFeedbackService : IUserFeedbackService
    {
        private readonly IUserFeedbackRepository _repo;
        public UserFeedbackService(IUserFeedbackRepository repo) => _repo = repo;

        public Task<UserFeedback> AddFeedbackAsync(UserFeedback feedback) => _repo.AddAsync(feedback);
        public Task<List<UserFeedback>> GetAllAsync() => _repo.GetAllAsync();
        public Task<double> GetAverageRatingAsync() => _repo.GetAverageRatingAsync();
    }
}
