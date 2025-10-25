using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestaurantService.API.Data.DBContext;
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
        private readonly Exe201RestaurantServiceDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public GooglePlacesService(HttpClient httpClient, IConfiguration configuration, IRestaurantRepository restaurantRepository,
            IReviewRepository reviewRepo, Exe201RestaurantServiceDbContext context, ICloudinaryService cloudinaryService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _restaurantRepository = restaurantRepository;
            _context = context;
            _apiKey = _configuration["GooglePlaces:ApiKey"];
            _baseUrl = _configuration["GooglePlaces:BaseUrl"];
            _reviewRepo = reviewRepo;
            _cloudinaryService = cloudinaryService;
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
            {
                string photoReference = place.Photos.First().PhotoReference;
                string photoUrl = await GetPhotoUrlAsync(photoReference, 800);
                coverImageUrl = await DownloadAndUploadToCloudinaryAsync(photoUrl);
            }

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

        public async Task<List<GooglePlaceDTO>> ImportDataAsync(GooglePlacesSearchRequest request)
        {
            // 1. Lấy danh sách địa điểm gần đó từ Google Places
            var places = await SearchNearbyRestaurantsAsync(request);
            var dtos = new List<GooglePlaceDTO>();

            foreach (var place in places)
            {
                // 2. Lấy hoặc tạo mới restaurant trong DB
                var restaurant = await _restaurantRepository.GetByGooglePlaceIdAsync(place.PlaceId);
                bool isNew = false;

                if (restaurant == null)
                {
                    restaurant = await MapGooglePlaceToRestaurantAsync(place);
                    restaurant = await _restaurantRepository.CreateAsync(restaurant);
                    isNew = true;
                }
                else
                {
                    // Cập nhật các trường còn thiếu hoặc thay đổi
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
                    if (place.Photos != null && place.Photos.Any())
                    {
                        if (place.Photos != null && place.Photos.Any())
                        {
                            string photoReference = place.Photos.First().PhotoReference;
                            string photoUrl = await GetPhotoUrlAsync(photoReference, 800);
                            string savedImageUrl = await DownloadAndUploadToCloudinaryAsync(photoUrl);

                            if (string.IsNullOrEmpty(restaurant.CoverImageUrl) || restaurant.CoverImageUrl != savedImageUrl)
                            {
                                restaurant.CoverImageUrl = savedImageUrl;
                                needUpdate = true;
                            }
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

                // 3. Mapping Categories (RestaurantCategory table)
                List<int> categoryIds = new List<int>();
                if (place.Types != null)
                {
                    var existedCategories = await _restaurantRepository.GetCategoriesByRestaurantIdAsync(restaurant.RestaurantId);
                    var existedCategoryIds = existedCategories.Select(x => x.CategoryId).ToList();

                    foreach (var type in place.Types)
                    {
                        var category = await _restaurantRepository.GetOrCreateCategoryByNameAsync(type, "Google");
                        if (!existedCategoryIds.Contains(category.CategoryId))
                        {
                            _context.RestaurantCategories.Add(new RestaurantCategory
                            {
                                RestaurantId = restaurant.RestaurantId,
                                CategoryId = category.CategoryId
                            });
                            existedCategoryIds.Add(category.CategoryId); // tránh trùng lặp khi lặp type
                        }
                    }
                    await _context.SaveChangesAsync();
                    categoryIds = existedCategoryIds;
                }

                // 4. Đồng bộ OpeningHours nếu có thay đổi
                if (place.OpeningHours?.WeekdayText?.Any() == true)
                {
                    var newOpeningHours = string.Join("; ", place.OpeningHours.WeekdayText);
                    if (restaurant.OpeningHours != newOpeningHours)
                    {
                        restaurant.OpeningHours = newOpeningHours;
                        await _restaurantRepository.UpdateAsync(restaurant);
                    }
                }

                // 5. Lưu Google reviews
                var reviews = await GetGoogleReviewsAsync(place.PlaceId);
                await _reviewRepo.AddOrUpdateReviewsAsync(restaurant.RestaurantId, reviews);

                // 6. Map DTO trả về
                var dto = new GooglePlaceDTO
                {
                    PlaceId = place.PlaceId,
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
                    Types = place.Types ?? new List<string>(),
                    CoverImageUrl = restaurant.CoverImageUrl,
                    Reviews = reviews,
                    PriceRangeId = restaurant.PriceRangeId,
                    CategoryIds = categoryIds
                };
                dtos.Add(dto);
            }
            return dtos;
        }

        public async Task<List<GooglePlaceDTO>> SearchAndImportNearbyAsync(GooglePlacesSearchRequest request)
        {
            var places = await SearchNearbyRestaurantsAsync(request);

            var dtos = new List<GooglePlaceDTO>();

            foreach (var place in places)
            {
                // 1. Get or Create restaurant
                var restaurant = await _restaurantRepository.GetByGooglePlaceIdAsync(place.PlaceId);

                if (restaurant == null)
                {
                    // Create new Restaurant
                    restaurant = await MapGooglePlaceToRestaurantAsync(place);
                    restaurant = await _restaurantRepository.CreateAsync(restaurant);
                }
                else
                {
                    // Update restaurant info if needed
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

                // 2. Đồng bộ Category (RestaurantCategory mapping)
                if (place.Types != null)
                {
                    // Lấy list category id đã có cho restaurant
                    var existedCategories = await _restaurantRepository.GetCategoriesByRestaurantIdAsync(restaurant.RestaurantId);
                    var existedCategoryIds = existedCategories.Select(x => x.CategoryId).ToList();

                    foreach (var type in place.Types)
                    {
                        var category = await _restaurantRepository.GetOrCreateCategoryByNameAsync(type, "Google");
                        if (!existedCategoryIds.Contains(category.CategoryId))
                        {
                            _context.RestaurantCategories.Add(new RestaurantCategory
                            {
                                RestaurantId = restaurant.RestaurantId,
                                CategoryId = category.CategoryId
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // 3. Đồng bộ OpeningHours nếu có thay đổi
                if (place.OpeningHours?.WeekdayText?.Any() == true)
                {
                    var newOpeningHours = string.Join("; ", place.OpeningHours.WeekdayText);
                    if (restaurant.OpeningHours != newOpeningHours)
                    {
                        restaurant.OpeningHours = newOpeningHours;
                        await _restaurantRepository.UpdateAsync(restaurant);
                    }
                }

                // 4. Đồng bộ reviews Google
                var reviews = await GetGoogleReviewsAsync(place.PlaceId);
                await _reviewRepo.AddOrUpdateReviewsAsync(restaurant.RestaurantId, reviews);

                // 5. Map lại DTO từ database (đảm bảo trả đúng PriceRangeId và CategoryIds)
                var categories = await _restaurantRepository.GetCategoriesByRestaurantIdAsync(restaurant.RestaurantId);
                var dto = new GooglePlaceDTO
                {
                    PlaceId = place.PlaceId,
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
                    Types = place.Types ?? new List<string>(),
                    CoverImageUrl = restaurant.CoverImageUrl,
                    Reviews = reviews,
                    PriceRangeId = restaurant.PriceRangeId,
                    CategoryIds = categories.Select(x => x.CategoryId).ToList()
                };
                dtos.Add(dto);
            }
            return dtos;
        }


        public async Task<GooglePlaceDTO> MapGooglePlaceToDto(GooglePlace place)
        {
            var reviews = await GetGoogleReviewsAsync(place.PlaceId);

            string coverImageUrl = null;
            if (place.Photos != null && place.Photos.Any())
                coverImageUrl = await GetPhotoUrlAsync(place.Photos.First().PhotoReference, 800);

            int? priceRangeId = null;
            List<int> categoryIds = new List<int>();

            var restaurant = await _restaurantRepository.GetByGooglePlaceIdAsync(place.PlaceId);
            if (restaurant != null)
            {
                priceRangeId = restaurant.PriceRangeId;
                var cats = await _restaurantRepository.GetCategoriesByRestaurantIdAsync(restaurant.RestaurantId);
                categoryIds = cats.Select(c => c.CategoryId).ToList();
            }
            else
            {
                // fallback nếu chưa có, tự map lại
                priceRangeId = place.PriceLevel != null
                    ? await _restaurantRepository.GetOrCreatePriceRangeIdAsync(place.PriceLevel)
                    : null;

                foreach (var type in place.Types ?? new List<string>())
                {
                    var cat = await _restaurantRepository.GetOrCreateCategoryByNameAsync(type, "Google");
                    if (!categoryIds.Contains(cat.CategoryId))
                        categoryIds.Add(cat.CategoryId);
                }
            }

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
                CoverImageUrl = coverImageUrl,
                PriceRangeId = priceRangeId,
                CategoryIds = categoryIds
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

        public async Task<bool> IsImageUrlValidAsync(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> DownloadAndUploadToCloudinaryAsync(string imageUrl)
        {
            // Download về thư mục tạm (giống như cũ)
            string tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "TempImages");
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
            string tempFile = Path.Combine(tempFolder, Guid.NewGuid().ToString("N") + ".jpg");

            using (var client = new HttpClient())
            {
                var bytes = await client.GetByteArrayAsync(imageUrl);
                await File.WriteAllBytesAsync(tempFile, bytes);
            }

            // Upload lên Cloudinary
            var cloudinaryService = await _cloudinaryService.UploadImageAsync(tempFile);
            // Xóa file tạm
            if (File.Exists(tempFile))
                File.Delete(tempFile);

            return cloudinaryService;
        }

    }
}
