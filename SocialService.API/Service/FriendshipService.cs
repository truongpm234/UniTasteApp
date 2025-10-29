using System.Net.Http.Json;
using SocialService.API.Models.DTO;
using SocialService.API.Repository;

namespace SocialService.API.Service
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IFriendshipRepository _repo;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public FriendshipService(IFriendshipRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task<IEnumerable<int>> GetFriendIdsAsync(int userId)
        {
            var friendships = await _repo.GetFriendsByUserIdAsync(userId);
            var friendIds = friendships
                .Select(f => f.UserId1 == userId ? (f.UserId2 ?? 0) : (f.UserId1 ?? 0))
                .Where(id => id != 0)
                .Distinct()
                .ToList();

            return friendIds;
        }

        public async Task<IEnumerable<int>> GetPendingRequesterIdsAsync(int userId)
        {
            var pendingRequests = await _repo.GetPendingRequestsAsync(userId);
            var requesterIds = pendingRequests
                .Select(f => f.UserId1 ?? 0)
                .Where(id => id != 0)
                .Distinct()
                .ToList();

            return requesterIds;
        }

        public async Task<Dictionary<int, int>> GetSuggestedFriendIdsAsync(int userId, string? token = null)
        {
            var friends = (await GetFriendIdsAsync(userId)).ToHashSet();
            var pending = (await GetPendingRequesterIdsAsync(userId)).ToHashSet();

            var gatewayUrl = _config["ServiceUrls:ApiGateway"];
            var request = new HttpRequestMessage(HttpMethod.Get, $"{gatewayUrl}/api/users/get-all");

            // ✅ Gắn token nếu có
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
            }

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"❌ Failed to fetch users: {response.StatusCode}");

            var allUsers = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            if (allUsers == null) return new Dictionary<int, int>();

            var mutualCounts = await _repo.GetSuggestedFriendIdsAsync(userId);

            var suggestions = allUsers
                .Where(u => u.UserId != userId && !friends.Contains(u.UserId) && !pending.Contains(u.UserId))
                .Select(u => new
                {
                    u.UserId,
                    MutualCount = mutualCounts.ContainsKey(u.UserId) ? mutualCounts[u.UserId] : 0
                })
                .OrderByDescending(x => x.MutualCount)
                .Take(10)
                .ToDictionary(x => x.UserId, x => x.MutualCount);

            Console.WriteLine($"[DEBUG] Suggested friends for {userId}: {string.Join(", ", suggestions.Select(s => $"{s.Key}({s.Value})"))}");

            return suggestions;
        }
    }
}
