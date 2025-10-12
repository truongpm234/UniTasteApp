namespace UserService.API.Services
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);

    }
}