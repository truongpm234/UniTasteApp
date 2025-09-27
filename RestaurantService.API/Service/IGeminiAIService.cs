namespace RestaurantService.API.Service
{
    public interface IGeminiAIService
    {
        Task<string> getChatResponse(string prompt);

    }
}