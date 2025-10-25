namespace RestaurantService.API.Service
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(string localFilePath, string fileName = null);
    }
}