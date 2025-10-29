using Microsoft.EntityFrameworkCore;
using SocialService.API.Data.DBContext;
using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly Exe201SocialServiceDbContext _context;

        public FriendshipRepository(Exe201SocialServiceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Friendship>> GetFriendsByUserIdAsync(int userId)
        {
            var friendships = await _context.Friendships
                .Where(f =>
                    f.Status != null &&
                    f.Status.Trim().ToLower() == "accepted" && 
                    (f.UserId1 == userId || f.UserId2 == userId)
                )
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Found {friendships.Count} friendships for user {userId}");
            foreach (var f in friendships)
            {
                Console.WriteLine($"  → FriendshipId={f.FriendshipId} | {f.UserId1} <-> {f.UserId2} | Status={f.Status}");
            }

            return friendships;
        }

        public async Task<IEnumerable<Friendship>> GetPendingRequestsAsync(int userId)
        {
            // 🟡 Lấy những lời mời mà user này là người được nhận (UserId2)
            return await _context.Friendships
                .Where(f =>
                    f.Status != null &&
                    f.Status.Trim().ToLower() == "pending" &&
                    f.UserId2 == userId)
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetSuggestedFriendIdsAsync(int userId)
        {
            var currentFriends = await _context.Friendships
                .Where(f => f.Status == "Accepted" &&
                            (f.UserId1 == userId || f.UserId2 == userId))
                .Select(f => f.UserId1 == userId ? f.UserId2 : f.UserId1)
                .ToListAsync();

            var pending = await _context.Friendships
                .Where(f => f.Status == "Pending" &&
                            (f.UserId1 == userId || f.UserId2 == userId))
                .Select(f => f.UserId1 == userId ? f.UserId2 : f.UserId1)
                .ToListAsync();

            var friendsOfFriends = await _context.Friendships
                .Where(f => f.Status == "Accepted" &&
                            (currentFriends.Contains(f.UserId1!.Value) ||
                             currentFriends.Contains(f.UserId2!.Value)))
                .ToListAsync();

            var mutualCounts = new Dictionary<int, int>();
            foreach (var f in friendsOfFriends)
            {
                var friendA = f.UserId1!.Value;
                var friendB = f.UserId2!.Value;

                if (friendA != userId &&
                    !currentFriends.Contains(friendA) &&
                    !pending.Contains(friendA))
                {
                    mutualCounts[friendA] = mutualCounts.GetValueOrDefault(friendA, 0) + 1;
                }

                if (friendB != userId &&
                    !currentFriends.Contains(friendB) &&
                    !pending.Contains(friendB))
                {
                    mutualCounts[friendB] = mutualCounts.GetValueOrDefault(friendB, 0) + 1;
                }
            }

            // 🧠 Fix lỗi EF Core bằng ToList trước
            var allUserIds = await _context.Friendships
                .Select(f => new { f.UserId1, f.UserId2 })
                .ToListAsync();

            var relatedIds = currentFriends.Concat(pending).ToHashSet();
            relatedIds.Add(userId);

            var unrelatedIds = allUserIds
                .SelectMany(f => new[] { f.UserId1 ?? 0, f.UserId2 ?? 0 })
                .Where(id => id != 0 && !relatedIds.Contains(id))
                .Distinct()
                .ToList();

            foreach (var id in unrelatedIds)
            {
                if (!mutualCounts.ContainsKey(id))
                    mutualCounts[id] = 0;
            }

            var sortedSuggestions = mutualCounts
                .Where(kvp => kvp.Key != 0)
                .OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Console.WriteLine($"[DEBUG] Friend suggestions for {userId}: {string.Join(", ", sortedSuggestions.Select(x => $"{x.Key}({x.Value})"))}");

            return sortedSuggestions;
        }


    }
}
