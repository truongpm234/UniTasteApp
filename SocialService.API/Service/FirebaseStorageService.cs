using Microsoft.Extensions.Options;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using SocialService.API.Models.DTO;

namespace SocialService.API.Service
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FirebaseStorageService(StorageClient storageClient, IOptions<FirebaseSettings> firebaseSettings)
        {
            _storageClient = storageClient;
            _bucketName = firebaseSettings.Value.BucketName;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Tệp tải lên trống hoặc không hợp lệ.");

            var uniqueFileName = $"{folderName}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            try
            {
                using var stream = file.OpenReadStream();

                await _storageClient.UploadObjectAsync(
                    bucket: _bucketName,
                    objectName: uniqueFileName,
                    contentType: file.ContentType,
                    source: stream
                );

                var encodedFileName = Uri.EscapeDataString(uniqueFileName);
                return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{encodedFileName}?alt=media";
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải tệp lên Firebase Storage: {ex.Message}");
            }
        }
    }
}
