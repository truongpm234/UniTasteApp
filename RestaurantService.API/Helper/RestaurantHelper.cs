using RestaurantService.API.Models.DTO;
using System.Collections.Generic;

public static class RestaurantHelper
{
    public static List<OpeningHourDto2> ParseOpeningHours(string openingHours)
    {
        var result = new List<OpeningHourDto2>();
        if (string.IsNullOrWhiteSpace(openingHours)) return result;
        var items = openingHours.Split(';');
        foreach (var item in items)
        {
            var parts = item.Split(':', 2);
            if (parts.Length == 2)
            {
                result.Add(new OpeningHourDto2
                {
                    Day = parts[0].Trim(),
                    Hours = parts[1].Trim()
                });
            }
        }
        return result;
    }
}
