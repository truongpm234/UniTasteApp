using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using global::RestaurantService.API.Models.DTO;
using Microsoft.Extensions.Options;
namespace RestaurantService.API.Service
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var settings = config.Value;
            Account account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(string localFilePath, string fileName = null)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(localFilePath),
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true,
                Folder = "restaurants", 
                PublicId = fileName 
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString();
            }
            else
            {
                throw new Exception("Cloudinary upload failed: " + uploadResult.Error?.Message);
            }
        }
    }

}
