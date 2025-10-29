using NPOI.SS.Formula.Functions;
using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public interface IFriendshipRepository
    {
        Task<IEnumerable<Friendship>> GetFriendsByUserIdAsync(int userId);
        Task<IEnumerable<Friendship>> GetPendingRequestsAsync(int userId);
        Task<Dictionary<int, int>> GetSuggestedFriendIdsAsync(int userId);
    }
}
