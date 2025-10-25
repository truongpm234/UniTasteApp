using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialService.API.Data.DBContext;
using SocialService.API.Models.DTO;
using SocialService.API.Models.Entity;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SocialService.API.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _connections = new();
        private static readonly ConcurrentDictionary<string, string> _activeChats = new(); // ai đang xem ai

        public override async Task OnConnectedAsync()
        {
            var httpCtx = Context.GetHttpContext();
            var username = httpCtx?.Request.Query["username"].ToString();
            var accessToken = httpCtx?.Request.Query["access_token"].ToString();

            if (!string.IsNullOrEmpty(username))
            {
                _connections[username] = Context.ConnectionId;
                Console.WriteLine($"✅ {username} connected with ID: {Context.ConnectionId}");

                // 🔥 Gửi danh sách user online cho người mới vào
                await Clients.Caller.SendAsync("OnlineUsers", _connections.Keys.ToList());

                // 🔥 Báo cho người khác biết user này vừa online
                await Clients.Others.SendAsync("UserOnline", username);

                using var scope = httpCtx!.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<Exe201SocialServiceDbContext>();

                using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8001") };
                if (!string.IsNullOrWhiteSpace(accessToken))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Lấy thông tin user
                var userResponse = await httpClient.GetAsync($"/api/users/getuser-by-fullname/{Uri.EscapeDataString(username)}");
                if (userResponse.IsSuccessStatusCode)
                {
                    var user = await userResponse.Content.ReadFromJsonAsync<UserDto>();

                    // Gửi lại các tin nhắn offline chưa gửi
                    var pendingMessages = await db.Messages
                        .Where(m => m.ReceiverId == user!.UserId && !m.IsDelivered)
                        .OrderBy(m => m.CreatedAt)
                        .ToListAsync();

                    foreach (var msg in pendingMessages)
                    {
                        var senderResp = await httpClient.GetAsync($"/api/users/getuser-by-id/{msg.SenderId}");
                        if (!senderResp.IsSuccessStatusCode) continue;

                        var sender = await senderResp.Content.ReadFromJsonAsync<UserDto>();
                        await Clients.Caller.SendAsync("ReceiveMessage", sender!.FullName, msg.Content);
                        msg.IsDelivered = true;
                    }

                    await db.SaveChangesAsync();
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (!string.IsNullOrEmpty(username))
            {
                _connections.TryRemove(username, out _);
                _activeChats.TryRemove(username, out _);

                Console.WriteLine($"❌ {username} disconnected.");

                // 🔥 Broadcast user offline cho tất cả client khác
                await Clients.Others.SendAsync("UserOffline", username);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ✅ Lưu trạng thái người đang xem ai
        public Task SetActiveChat(string viewer, string chattingWith)
        {
            if (string.IsNullOrEmpty(viewer))
                return Task.CompletedTask;

            if (string.IsNullOrEmpty(chattingWith))
            {
                _activeChats.TryRemove(viewer, out _);
                Console.WriteLine($"🚪 {viewer} đã rời khỏi khung chat.");
            }
            else
            {
                _activeChats[viewer] = chattingWith;
                Console.WriteLine($"💬 {viewer} đang xem khung chat với {chattingWith}.");
            }

            return Task.CompletedTask;
        }

        // ✅ Gửi tin nhắn
        public async Task SendMessage(string toUser, string message)
        {
            var httpCtx = Context.GetHttpContext();
            var fromUser = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            var accessToken = httpCtx?.Request.Query["access_token"].ToString();
            if (string.IsNullOrEmpty(fromUser)) fromUser = "Người lạ";

            using var scope = httpCtx!.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Exe201SocialServiceDbContext>();

            using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8001") };
            if (!string.IsNullOrWhiteSpace(accessToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var senderResponse = await httpClient.GetAsync($"/api/users/getuser-by-fullname/{Uri.EscapeDataString(fromUser)}");
            var receiverResponse = await httpClient.GetAsync($"/api/users/getuser-by-fullname/{Uri.EscapeDataString(toUser)}");

            if (senderResponse.IsSuccessStatusCode && receiverResponse.IsSuccessStatusCode)
            {
                var sender = await senderResponse.Content.ReadFromJsonAsync<UserDto>();
                var receiver = await receiverResponse.Content.ReadFromJsonAsync<UserDto>();

                bool receiverOnline = _connections.ContainsKey(toUser);
                bool receiverIsViewingChat = _activeChats.TryGetValue(toUser, out var viewing) && viewing == fromUser;

                // ✅ Lưu vào DB dù người nhận offline
                db.Messages.Add(new Message
                {
                    SenderId = sender!.UserId,
                    ReceiverId = receiver!.UserId,
                    Content = message,
                    CreatedAt = DateTime.UtcNow,
                    IsDelivered = receiverOnline,
                    IsSeen = receiverIsViewingChat
                });
                await db.SaveChangesAsync();

                // ✅ Gửi tin cho người gửi (hiển thị ngay trên UI)
                await Clients.Caller.SendAsync("ReceiveMessage", "Bạn", message);

                // ✅ Nếu người nhận đang online → gửi realtime
                if (receiverOnline && _connections.TryGetValue(toUser, out var receiverConnId))
                {
                    await Clients.Client(receiverConnId).SendAsync("ReceiveMessage", fromUser, message);
                }
            }
        }

        // ✅ Đánh dấu đã xem
        public async Task MarkAsSeen(string fromUser, string toUser)
        {
            using var scope = Context.GetHttpContext()!.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Exe201SocialServiceDbContext>();

            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8001") };
            var senderResp = await httpClient.GetAsync($"/api/users/getuser-by-fullname/{Uri.EscapeDataString(fromUser)}");
            var receiverResp = await httpClient.GetAsync($"/api/users/getuser-by-fullname/{Uri.EscapeDataString(toUser)}");

            if (senderResp.IsSuccessStatusCode && receiverResp.IsSuccessStatusCode)
            {
                var sender = await senderResp.Content.ReadFromJsonAsync<UserDto>();
                var receiver = await receiverResp.Content.ReadFromJsonAsync<UserDto>();

                var messages = await db.Messages
                    .Where(m => m.SenderId == sender!.UserId && m.ReceiverId == receiver!.UserId && !m.IsSeen)
                    .ToListAsync();

                foreach (var msg in messages)
                    msg.IsSeen = true;

                await db.SaveChangesAsync();

                Console.WriteLine($"👁 Tin nhắn từ {fromUser} → {toUser} đã được đánh dấu là 'Đã xem'.");

                if (_connections.TryGetValue(fromUser, out var senderConnId))
                {
                    await Clients.Client(senderConnId).SendAsync("MessagesSeen", toUser);
                }
            }
        }

        public async Task UserTyping(string fromUser, string toUser)
        {
            if (_connections.TryGetValue(toUser, out var receiverConnId))
            {
                await Clients.Client(receiverConnId).SendAsync("ShowTyping", fromUser, true);
            }
        }

        public async Task UserStopTyping(string fromUser, string toUser)
        {
            if (_connections.TryGetValue(toUser, out var receiverConnId))
            {
                await Clients.Client(receiverConnId).SendAsync("ShowTyping", fromUser, false);
            }
        }
    }
}
