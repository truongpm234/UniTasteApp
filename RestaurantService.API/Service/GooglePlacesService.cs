using Newtonsoft.Json;
using RestaurantService.API.Models.DTO;
using RestaurantService.API.Models.Entity;
using RestaurantService.API.Models.GooglePlaces;
using RestaurantService.API.Repository;

namespace RestaurantService.API.Service
{
    public class GooglePlacesService : IGooglePlacesService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IReviewRepository _reviewRepo;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public GooglePlacesService(HttpClient httpClient, IConfiguration configuration, IRestaurantRepository restaurantRepository, IReviewRepository reviewRepo)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _restaurantRepository = restaurantRepository;
            _apiKey = _configuration["GooglePlaces:ApiKey"];
            _baseUrl = _configuration["GooglePlaces:BaseUrl"];
            _reviewRepo = reviewRepo;
        }

        public int MapPriceLevelToPriceRangeId(int? priceLevel)
        {
            if (priceLevel == null) return 1;
            switch (priceLevel)
            {
                case 0: return 1;
                case 1: return 2;
                case 2: return 3;
                case 3: return 4;
                case 4: return 5;
                default: return 1;
            }
        }
        public async Task<int> SyncAllRestaurantCoverImagesAsync()
        {
            // Lấy toàn bộ restaurant chưa có cover
            var allRestaurants = await _restaurantRepository.GetAllRestaurantAsync();
            int updated = 0;
            foreach (var r in allRestaurants)
            {
                if (string.IsNullOrEmpty(r.CoverImageUrl) && !string.IsNullOrEmpty(r.GooglePlaceId))
                {
                    // Lấy lại chi tiết từ Google Place API
                    var place = await GetPlaceDetailAsync(r.GooglePlaceId);
                    if (place?.Photos != null && place.Photos.Any())
                    {
                        r.CoverImageUrl = await GetPhotoUrlAsync(place.Photos.First().PhotoReference, 800);
                        await _restaurantRepository.UpdateAsync(r);
                        updated++;
                    }
                }
            }
            return updated; // trả về số lượng đã update
        }
        public async Task<Restaurant> MapGooglePlaceToRestaurantAsync(GooglePlace place)
        {
            if (place == null) throw new ArgumentNullException(nameof(place));

            string openingHoursStr = "Chưa cập nhật";
            if (place.OpeningHours != null && place.OpeningHours.WeekdayText?.Count > 0)
                openingHoursStr = string.Join("; ", place.OpeningHours.WeekdayText);

            int priceRangeId = (int)(await _restaurantRepository.GetOrCreatePriceRangeIdAsync(place.PriceLevel));

            string coverImageUrl = null;
            if (place.Photos != null && place.Photos.Any())
                coverImageUrl = await GetPhotoUrlAsync(place.Photos.First().PhotoReference, 800);

            return new Restaurant
            {
                Name = place.Name ?? "Chưa cập nhật",
                Address = place.FormattedAddress ?? place.Vicinity ?? "Chưa cập nhật",
                Latitude = place.Geometry?.Location?.Lat ?? 0,
                Longitude = place.Geometry?.Location?.Lng ?? 0,
                GooglePlaceId = place.PlaceId ?? "",
                Phone = place.FormattedPhoneNumber ?? "Chưa cập nhật",
                Website = place.Website ?? "Chưa cập nhật",
                CoverImageUrl = coverImageUrl,
                GoogleRating = place.Rating ?? 0,
                OpeningHours = openingHoursStr,
                PriceRangeId = priceRangeId,
                CreatedAt = DateTime.UtcNow,
                Status = "Active"
            };
        }

        public async Task<List<GooglePlace>> SearchNearbyRestaurantsAsync(GooglePlacesSearchRequest request)
        {
            var url = BuildNearbySearchUrl(request, null);
            var response = await _httpClient.GetStringAsync(url);
            var placesResponse = JsonConvert.DeserializeObject<GooglePlacesResponse>(response);

            var places = placesResponse?.Results ?? new List<GooglePlace>();
            var tasks = places.Select(p => GetPlaceDetailAsync(p.PlaceId)).ToList();
            var placesWithDetails = await Task.WhenAll(tasks);

            return placesWithDetails.Where(x => x != null).ToList();
        }

        public async Task<List<GooglePlaceDTO>> SearchAndImportNearbyAsync(GooglePlacesSearchRequest request)
        {
            var places = await SearchNearbyRestaurantsAsync(request);
            foreach (var place in places)
            {
                var restaurant = await _restaurantRepository.GetByGooglePlaceIdAsync(place.PlaceId);

                if (restaurant == null)
                {
                    // Create new, always map cover from photo_reference
                    restaurant = await MapGooglePlaceToRestaurantAsync(place);
                    await _restaurantRepository.CreateAsync(restaurant);
                }
                else
                {
                    // Update missing or changed fields
                    bool needUpdate = false;
                    if (string.IsNullOrEmpty(restaurant.Address) && !string.IsNullOrEmpty(place.FormattedAddress))
                    { restaurant.Address = place.FormattedAddress; needUpdate = true; }
                    if (restaurant.Latitude == null && place.Geometry?.Location?.Lat != null)
                    { restaurant.Latitude = place.Geometry.Location.Lat; needUpdate = true; }
                    if (restaurant.Longitude == null && place.Geometry?.Location?.Lng != null)
                    { restaurant.Longitude = place.Geometry.Location.Lng; needUpdate = true; }
                    if (string.IsNullOrEmpty(restaurant.Phone) && !string.IsNullOrEmpty(place.FormattedPhoneNumber))
                    { restaurant.Phone = place.FormattedPhoneNumber; needUpdate = true; }
                    if (string.IsNullOrEmpty(restaurant.Website) && !string.IsNullOrEmpty(place.Website))
                    { restaurant.Website = place.Website; needUpdate = true; }

                    // Always update CoverImageUrl if photo_reference changed or db null
                    if (place.Photos != null && place.Photos.Any())
                    {
                        var newCover = await GetPhotoUrlAsync(place.Photos.First().PhotoReference, 800);
                        if (string.IsNullOrEmpty(restaurant.CoverImageUrl) || restaurant.CoverImageUrl != newCover)
                        {
                            restaurant.CoverImageUrl = newCover;
                            needUpdate = true;
                        }
                    }

                    if ((restaurant.GoogleRating == null || restaurant.GoogleRating == 0) && place.Rating != null)
                    { restaurant.GoogleRating = place.Rating; needUpdate = true; }
                    if (string.IsNullOrEmpty(restaurant.OpeningHours) && place.OpeningHours?.WeekdayText?.Any() == true)
                    { restaurant.OpeningHours = string.Join("; ", place.OpeningHours.WeekdayText); needUpdate = true; }
                    if ((restaurant.PriceRangeId == null || restaurant.PriceRangeId == 1) && place.PriceLevel != null)
                    { restaurant.PriceRangeId = await _restaurantRepository.GetOrCreatePriceRangeIdAsync(place.PriceLevel); needUpdate = true; }
                    if (needUpdate)
                        await _restaurantRepository.UpdateAsync(restaurant);
                }

                // Đồng bộ category, openinghour, review (giữ nguyên code cũ)
                if (place.Types != null)
                {
                    foreach (var type in place.Types)
                    {
                        var category = await _restaurantRepository.GetOrCreateCategoryByNameAsync(type, "Google");
                        if (!restaurant.Categories.Any(c => c.CategoryId == category.CategoryId))
                        {
                            restaurant.Categories.Add(category);
                        }
                    }
                    await _restaurantRepository.UpdateAsync(restaurant);
                }

                if (place.OpeningHours?.WeekdayText?.Any() == true)
                {
                    restaurant.OpeningHours = string.Join("; ", place.OpeningHours.WeekdayText);
                    await _restaurantRepository.UpdateAsync(restaurant);
                }

                var reviews = await GetGoogleReviewsAsync(place.PlaceId);
                await _reviewRepo.AddOrUpdateReviewsAsync(restaurant.RestaurantId, reviews);
            }

            var tasks = places.Select(place => MapGooglePlaceToDto(place)).ToList();
            var result = await Task.WhenAll(tasks);
            return result.ToList();
        }

        public async Task<GooglePlaceDTO> MapGooglePlaceToDto(GooglePlace place)
        {
            var reviews = await GetGoogleReviewsAsync(place.PlaceId);

            string coverImageUrl = null;
            if (place.Photos != null && place.Photos.Any())
                coverImageUrl = await GetPhotoUrlAsync(place.Photos.First().PhotoReference, 800);

            return new GooglePlaceDTO
            {
                PlaceId = place.PlaceId,
                Reviews = reviews,
                Name = place.Name ?? "Chưa cập nhật",
                FormattedAddress = place.FormattedAddress ?? place.Vicinity ?? "Chưa cập nhật",
                Latitude = place.Geometry?.Location?.Lat ?? 0,
                Longitude = place.Geometry?.Location?.Lng ?? 0,
                Website = place.Website ?? "Chưa cập nhật",
                Phone = place.FormattedPhoneNumber ?? "Chưa cập nhật",
                PriceLevel = place.PriceLevel?.ToString() ?? "Chưa cập nhật",
                Rating = place.Rating ?? 0,
                OpeningHours = (place.OpeningHours?.WeekdayText?.Count > 0)
                    ? string.Join("; ", place.OpeningHours.WeekdayText)
                    : "Chưa cập nhật",
                Types = place.Types,
                CoverImageUrl = coverImageUrl
            };
        }

        public async Task<GooglePlace> GetPlaceDetailAsync(string placeId)
        {
            var url = $"{_baseUrl}/details/json?place_id={placeId}&fields=place_id,name,formatted_address,geometry,formatted_phone_number,website,opening_hours,types,photos,price_level,rating,vicinity,reviews&key={_apiKey}&language=vi";
            var response = await _httpClient.GetStringAsync(url);
            var detail = JsonConvert.DeserializeObject<GooglePlaceDetailsResponse>(response);

            return detail?.Result;
        }

        public string BuildNearbySearchUrl(GooglePlacesSearchRequest request, string nextPageToken = null)
        {
            var url = $"{_baseUrl}/nearbysearch/json?location={request.Latitude},{request.Longitude}&radius={request.Radius}&type={request.Type}&key={_apiKey}&language=vi";
            if (!string.IsNullOrEmpty(request.Keyword))
                url += $"&keyword={Uri.EscapeDataString(request.Keyword)}";
            if (!string.IsNullOrEmpty(nextPageToken))
                url += $"&pagetoken={nextPageToken}";
            return url;
        }

        public async Task<string> GetPhotoUrlAsync(string photoReference, int maxWidth = 400)
        {
            if (string.IsNullOrEmpty(photoReference))
                return null;
            return $"{_baseUrl}/photo?photo_reference={photoReference}&maxwidth={maxWidth}&key={_apiKey}";
        }

        public async Task<List<GoogleReview>> GetGoogleReviewsAsync(string googlePlaceId)
        {
            var url = $"{_baseUrl}/details/json?place_id={googlePlaceId}&fields=reviews&key={_apiKey}&language=vi";
            var response = await _httpClient.GetStringAsync(url);
            var detail = JsonConvert.DeserializeObject<GooglePlaceDetailsResponse>(response);
            return detail?.Result?.Reviews ?? new List<GoogleReview>();
        }
    }
}
