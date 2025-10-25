using Microsoft.AspNetCore.Mvc;
using SocialService.API.Service;

namespace SocialService.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        // ✅ Lấy lịch sử chat giữa 2 người
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(int senderId, int receiverId)
        {
            var messages = await _messageService.GetMessagesBetweenUsersAsync(senderId, receiverId);
            return Ok(messages);
        }

        // ✅ Lấy tin nhắn offline chưa đọc
        [HttpGet("offline/{receiverId}")]
        public async Task<IActionResult> GetOfflineMessages(int receiverId)
        {
            var messages = await _messageService.GetOfflineMessagesAsync(receiverId);
            return Ok(messages);
        }

        [HttpPut("user-seen-message")]
        public async Task<IActionResult> MarkAsSeen(int senderId, int receiverId)
        {
            await _messageService.MarkMessagesAsSeenAsync(senderId, receiverId);
            return Ok(new { message = "Đã đánh dấu tin nhắn là đã xem." });
        }

    }
}
