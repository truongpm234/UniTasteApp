using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using global::UserService.API.Models.DTO;
using Microsoft.Extensions.Options;


namespace UserService.API.Services
{  
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        // Upload IFormFile, trả về public URL
        public async Task<string> UploadAvatarAsync(IFormFile file, string folder = "avatars")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty!");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder, // lưu trong folder "avatars"
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true,
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                return uploadResult.SecureUrl.ToString();
            throw new Exception(uploadResult.Error?.Message ?? "Upload failed");
        }
    }

}
