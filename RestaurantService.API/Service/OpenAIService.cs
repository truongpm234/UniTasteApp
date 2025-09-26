//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Options;
//using OpenAI;
//using OpenAI.Chat;
//using OpenAI.Models;
//using RestaurantService.API.Models.DTO;
//using System.Data;

//namespace RestaurantService.API.Service
//{
//    public class OpenAIService
//    {
//        private readonly OpenAIClient _client;

//        public OpenAIService(IOptions<OpenAIOptions> options)
//        {
//            _client = new OpenAIClient(new OpenAIAuthentication(options.Value.ApiKey));
//        }

//        // Đề xuất quán ăn dựa trên lịch sử và danh sách quán
//        public async Task<string> RecommendRestaurantsAsync(string userHistory, string search, List<string> restaurantNames)
//        {
//            var prompt = $"Dựa trên lịch sử tìm kiếm/sở thích của tôi: {userHistory}, và tôi vừa tìm: '{search}'. Đây là danh sách các quán: {string.Join(", ", restaurantNames)}. Hãy gợi ý cho tôi 3 quán phù hợp nhất, nêu ngắn gọn lý do (bằng tiếng Việt).";

//            var chatMessages = new List<ChatMessage>
//        {
//            ChatMessage.System("Bạn là AI tư vấn chọn quán ăn cho người dùng."),
//            ChatMessage.User(prompt)
//        };

//            var chatRequest = new ChatRequest(chatMessages, Model.GPT3_5_Turbo);
//            var response = await _client.ChatEndpoint.GetCompletionAsync(chatRequest);
//            return response.FirstChoice.Message.Content.Trim();
//        }

//        // Tóm tắt review cho 1 quán ăn
//        public async Task<string> SummarizeReviewsAsync(string restaurantName, IEnumerable<string> reviews)
//        {
//            var prompt = $"Hãy tóm tắt các ý chính về trải nghiệm khách hàng của quán '{restaurantName}' từ các review sau bằng tiếng Việt, nêu bật ưu điểm/nhược điểm nổi bật (không quá 100 từ):\n\n"
//                + string.Join("\n", reviews.Select((r, i) => $"- {r}"));

//            var chatMessages = new List<ChatMessage>
//        {
//            ChatMessage.System("Bạn là AI chuyên tóm tắt review quán ăn."),
//            ChatMessage.User(prompt)
//        };

//            var chatRequest = new ChatRequest(chatMessages, Model.GPT3_5_Turbo);
//            var response = await _client.ChatEndpoint.GetCompletionAsync(chatRequest);
//            return response.FirstChoice.Message.Content.Trim();
//        }
//    }
//}
