using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(int userId1, int userId2);
        Task<IEnumerable<Message>> GetUndeliveredMessagesAsync(int userId);
        Task AddMessageAsync(Message message);
        Task MarkMessagesAsSeenAsync(int senderId, int receiverId);
        Task SaveChangesAsync();
    }
}
