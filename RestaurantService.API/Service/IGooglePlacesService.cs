using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Models.GooglePlaces;

namespace RestaurantService.API.Service
{
    public interface IGooglePlacesService
    {
        Task<List<GooglePlace>> SearchNearbyRestaurantsAsync(GooglePlacesSearchRequest request);
        string BuildNearbySearchUrl(GooglePlacesSearchRequest request, string nextPageToken = null);
        Task<string> GetPhotoUrlAsync(string photoReference, int maxWidth = 400);
        Task<GooglePlaceDTO> MapGooglePlaceToDto(GooglePlace place);
        Task<List<GooglePlaceDTO>> SearchAndImportNearbyAsync(GooglePlacesSearchRequest request);
        Task<List<GoogleReview>> GetGoogleReviewsAsync(string googlePlaceId);
        int MapPriceLevelToPriceRangeId(int? priceLevel);
        Task<int> SyncAllRestaurantCoverImagesAsync();


    }
}