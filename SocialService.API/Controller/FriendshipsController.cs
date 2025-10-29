using Microsoft.AspNetCore.Mvc;
using SocialService.API.Models.DTO;
using SocialService.API.Service;

namespace SocialService.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendshipsController : ControllerBase
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IConfiguration _config;

        public FriendshipsController(IFriendshipService friendshipService, IConfiguration config)
        {
            _friendshipService = friendshipService;
            _config = config;
        }

        [HttpGet("user/{userId}/details")]
        public async Task<IActionResult> GetFriendDetails(int userId)
        {
            var friendIds = await _friendshipService.GetFriendIdsAsync(userId);
            var gatewayUrl = _config["ServiceUrls:ApiGateway"];

            using var httpClient = new HttpClient { BaseAddress = new Uri(gatewayUrl) };
            var userList = new List<UserDto>();

            foreach (var id in friendIds)
            {
                var res = await httpClient.GetAsync($"/api/users/getuser-by-id/{id}");
                if (res.IsSuccessStatusCode)
                {
                    var user = await res.Content.ReadFromJsonAsync<UserDto>();
                    if (user != null)
                        userList.Add(user);
                }
            }

            return Ok(userList);
        }

        [HttpGet("pending/{userId}")]
        public async Task<IActionResult> GetPendingRequests(int userId)
        {
            var requesterIds = await _friendshipService.GetPendingRequesterIdsAsync(userId);
            var gatewayUrl = _config["ServiceUrls:ApiGateway"];

            using var httpClient = new HttpClient { BaseAddress = new Uri(gatewayUrl) };
            var userList = new List<UserDto>();

            foreach (var id in requesterIds)
            {
                var res = await httpClient.GetAsync($"/api/users/getuser-by-id/{id}");
                if (res.IsSuccessStatusCode)
                {
                    var user = await res.Content.ReadFromJsonAsync<UserDto>();
                    if (user != null)
                        userList.Add(user);
                }
            }

            return Ok(userList);
        }

        [HttpGet("suggestions/{userId}")]
        public async Task<IActionResult> GetFriendSuggestions(int userId)
        {
            // ✅ Lấy token từ request
            string? token = HttpContext.Request.Headers["Authorization"];

            var suggested = await _friendshipService.GetSuggestedFriendIdsAsync(userId, token);
            var gatewayUrl = _config["ServiceUrls:ApiGateway"];

            using var httpClient = new HttpClient { BaseAddress = new Uri(gatewayUrl) };
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
            }

            var userList = new List<dynamic>();

            foreach (var kvp in suggested)
            {
                var id = kvp.Key;
                var mutualCount = kvp.Value;

                var res = await httpClient.GetAsync($"/api/users/getuser-by-id/{id}");
                if (res.IsSuccessStatusCode)
                {
                    var user = await res.Content.ReadFromJsonAsync<UserDto>();
                    if (user != null)
                    {
                        userList.Add(new
                        {
                            user.UserId,
                            user.FullName,
                            user.AvatarUrl,
                            MutualFriends = mutualCount
                        });
                    }
                }
            }

            return Ok(userList);
        }


    }
}
