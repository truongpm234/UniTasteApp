using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using SocialService.API.Models.DTO;


namespace SocialService.API.Service
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

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "posts")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty!");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = false,
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                return uploadResult.SecureUrl.ToString();
            throw new Exception(uploadResult.Error?.Message ?? "Upload failed");
        }

        // Upload nhiều ảnh
        public async Task<List<string>> UploadImagesAsync(List<IFormFile> files, string folder = "posts")
        {
            var urls = new List<string>();
            foreach (var file in files)
            {
                var url = await UploadImageAsync(file, folder);
                urls.Add(url);
            }
            return urls;
        }
    }

}
