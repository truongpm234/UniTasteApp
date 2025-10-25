using SocialService.API.Models.Entity;

namespace SocialService.API.Service
{
    public interface IMessageService
    {
        Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(int senderId, int receiverId);
        Task<IEnumerable<Message>> GetOfflineMessagesAsync(int receiverId);
        Task MarkMessagesAsSeenAsync(int senderId, int receiverId);
        Task AddMessageAsync(Message message);
    }
}
