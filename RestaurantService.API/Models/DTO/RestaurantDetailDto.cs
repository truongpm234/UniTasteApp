namespace RestaurantService.API.Models.DTO
{
    public class RestaurantDetailDto
    {
        public List<OpeningHourDto> OpeningHours { get; set; } = new();
        public int RestaurantId { get; set; }

        public string? Name { get; set; }

        public string? Address { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string? GooglePlaceId { get; set; }

        public string? Phone { get; set; }

        public string? Website { get; set; }

        public string? CoverImageUrl { get; set; }

        public double? GoogleRating { get; set; }

        public string? Opening { get; set; }

        public int? PriceRangeId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? Status { get; set; }
    }

    public class OpeningHourDto
    {
        public int? DayOfWeek { get; set; }
        public string DayName => DayOfWeek switch
        {
            0 => "Chủ nhật",
            1 => "Thứ 2",
            2 => "Thứ 3",
            3 => "Thứ 4",
            4 => "Thứ 5",
            5 => "Thứ 6",
            6 => "Thứ 7",
            _ => ""
        };
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
    }

}
