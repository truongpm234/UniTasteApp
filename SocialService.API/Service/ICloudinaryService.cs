namespace SocialService.API.Service
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "posts");
        Task<List<string>> UploadImagesAsync(List<IFormFile> files, string folder = "posts");

    }
}