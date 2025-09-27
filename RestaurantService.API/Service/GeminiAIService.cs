using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using RestaurantService.API.Models.DTO;
using System.Data;
using System.Text;
using System.Text.Json;

namespace RestaurantService.API.Service
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiAIService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["GeminiAI:ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("GeminiAI:ApiKey not configured in appsettings.json");
        }

        public async Task<string> getChatResponse(string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var requestJson = new HttpRequestMessage(
                HttpMethod.Post,
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent");
            requestJson.Headers.Add("X-goog-Api-Key", _apiKey);
            requestJson.Content = new StringContent(
                JsonSerializer.Serialize(requestBody), 
                Encoding.UTF8, "application/json"
                );
            var response = await _httpClient.SendAsync(requestJson);
            if (response.IsSuccessStatusCode)
            {
                using var responseContent = await response.Content.ReadAsStreamAsync();
                using var jsonDoc = await JsonDocument.ParseAsync(responseContent);

                var text = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
                return text ?? "No response";
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\nBody: {errorBody}");
            }
        }
    }
}
