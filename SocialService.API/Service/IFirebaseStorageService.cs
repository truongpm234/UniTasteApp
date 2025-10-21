namespace SocialService.API.Service
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
    }
}
