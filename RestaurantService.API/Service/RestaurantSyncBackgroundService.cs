//namespace RestaurantService.API.Service
//{
//    public class RestaurantSyncBackgroundService : BackgroundService
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private readonly ILogger<RestaurantSyncBackgroundService> _logger;
//        private readonly Timer _timer;

//        public RestaurantSyncBackgroundService(
//            IServiceProvider serviceProvider,
//            ILogger<RestaurantSyncBackgroundService> logger)
//        {
//            _serviceProvider = serviceProvider;
//            _logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    using var scope = _serviceProvider.CreateScope();
//                    var googlePlacesService = scope.ServiceProvider.GetRequiredService<IGooglePlacesService>();

//                    // Sync restaurants cho các thành phố chính (có thể config từ DB)
//                    var cities = new[]
//                    {
//                    new { Name = "Ho Chi Minh City", Lat = 10.8231, Lng = 106.6297 },
//                    new { Name = "Hanoi", Lat = 21.0285, Lng = 105.8542 }
//                };

//                    foreach (var city in cities)
//                    {
//                        await googlePlacesService.SyncRestaurantsToDbAsync(city.Lat, city.Lng);
//                        _logger.LogInformation($"Synced restaurants for {city.Name}");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Error occurred during restaurant sync");
//                }

//                // Chạy mỗi 6 tiếng
//                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
//            }
//        }
//    }
//}