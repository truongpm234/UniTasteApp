namespace UserService.API.Service
{
    public interface IGeminiAIService
    {
        Task<string> getChatResponse(string prompt);

    }
}