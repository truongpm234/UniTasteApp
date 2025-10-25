using Microsoft.EntityFrameworkCore;
using SocialService.API.Data.DBContext;
using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly Exe201SocialServiceDbContext _db;

        public MessageRepository(Exe201SocialServiceDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Message>> GetMessagesBetweenUsersAsync(int userId1, int userId2)
        {
            return await _db.Messages
                .Where(m =>
                    (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                    (m.SenderId == userId2 && m.ReceiverId == userId1))
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetUndeliveredMessagesAsync(int userId)
        {
            return await _db.Messages
                .Where(m => m.ReceiverId == userId && !m.IsDelivered)
                .ToListAsync();
        }

        public async Task AddMessageAsync(Message message)
        {
            await _db.Messages.AddAsync(message);
        }

        public async Task MarkMessagesAsSeenAsync(int senderId, int receiverId)
        {
            var unseenMessages = await _db.Messages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsSeen)
                .ToListAsync();

            if (unseenMessages.Any())
            {
                foreach (var msg in unseenMessages)
                    msg.IsSeen = true;

                await _db.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
