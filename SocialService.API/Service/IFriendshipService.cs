using System.Threading.Tasks;

namespace SocialService.API.Service
{
    public interface IFriendshipService
    {
        Task<IEnumerable<int>> GetFriendIdsAsync(int userId);
        Task<IEnumerable<int>> GetPendingRequesterIdsAsync(int userId);
        Task<Dictionary<int, int>> GetSuggestedFriendIdsAsync(int userId, string? token = null);

    }
}
