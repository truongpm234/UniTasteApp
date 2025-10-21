using System.Threading.Tasks;

namespace SocialService.API.Service
{
    public interface IPostReactionService
    {
        Task<string> ToggleReactionAsync(int userId, int postId, string reactionType);
        Task<object> CheckUserReactionAsync(int userId, int postId);
        Task<object> GetPostReactionsSummaryAsync(int postId);
    }
}
