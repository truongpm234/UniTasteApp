using SocialService.API.Models.DTO;

namespace SocialService.API.Service
{
    public interface IPostShareService
    {
        Task<int> SharePostAsync(PostShareCreateDto dto, int userId);
    }
}
