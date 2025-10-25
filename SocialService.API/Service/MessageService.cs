using SocialService.API.Models.Entity;
using SocialService.API.Repository;

namespace SocialService.API.Service
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _repo;

        public MessageService(IMessageRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(int senderId, int receiverId)
        {
            return await _repo.GetMessagesBetweenUsersAsync(senderId, receiverId);
        }

        public async Task<IEnumerable<Message>> GetOfflineMessagesAsync(int receiverId)
        {
            return await _repo.GetUndeliveredMessagesAsync(receiverId);
        }

        public async Task AddMessageAsync(Message message)
        {
            await _repo.AddMessageAsync(message);
            await _repo.SaveChangesAsync();
        }

        public async Task MarkMessagesAsSeenAsync(int senderId, int receiverId)
        {
            await _repo.MarkMessagesAsSeenAsync(senderId, receiverId);
        }

    }
}
