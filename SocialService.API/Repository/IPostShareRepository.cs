using SocialService.API.Models.Entity;

namespace SocialService.API.Repository
{
    public interface IPostShareRepository
    {
        Task AddShareAsync(PostShare share);
        Task SaveAsync();
    }
}
