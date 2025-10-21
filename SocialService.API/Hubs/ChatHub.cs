using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace SocialService.API.Hubs
{
    public class ChatHub : Hub
    {
        // Lưu username và connectionId
        private static readonly ConcurrentDictionary<string, string> _connections = new();

        public override Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext()?.Request.Query["username"];

            if (!string.IsNullOrEmpty(username))
            {
                _connections[username] = Context.ConnectionId;
                Console.WriteLine($"✅ {username} connected with ID: {Context.ConnectionId}");
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var username = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (!string.IsNullOrEmpty(username))
            {
                _connections.TryRemove(username, out _);
                Console.WriteLine($"❌ {username} disconnected.");
            }

            return base.OnDisconnectedAsync(exception);
        }

        // Gửi tin nhắn riêng tới người nhận
        public async Task SendMessage(string toUser, string message)
        {
            var fromUser = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

            if (string.IsNullOrEmpty(fromUser))
                fromUser = "Người lạ";

            if (_connections.TryGetValue(toUser, out var receiverConnectionId))
            {
                // Gửi cho người nhận
                await Clients.Client(receiverConnectionId)
                    .SendAsync("ReceiveMessage", fromUser, message);

                // Gửi lại cho người gửi (để hiển thị bên mình)
                await Clients.Caller.SendAsync("ReceiveMessage", "Bạn", message);
            }
            else
            {
                // Nếu người nhận offline
                await Clients.Caller.SendAsync("ReceiveMessage", "Hệ thống", $"{toUser} hiện không trực tuyến.");
            }
        }
    }
}
