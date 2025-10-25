namespace UserService.API.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadAvatarAsync(IFormFile file, string folder = "avatars");
    }
}