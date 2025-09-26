//using Microsoft.AspNetCore.Mvc;
//using RestaurantService.API.Service;
//using RestaurantService.API.Repository;
//using RestaurantService.API.Models.Entity;

//namespace RestaurantService.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AIController : ControllerBase
//    {
//        private readonly OpenAIService _openAIService;

//        public AIController(OpenAIService openAIService)
//        {
//            _openAIService = openAIService;
//        }

//        // API đề xuất quán ăn
//        [HttpPost("recommend")]
//        public async Task<IActionResult> Recommend([FromBody] RecommendRequest req)
//        {
//            var result = await _openAIService.RecommendRestaurantsAsync(req.UserHistory, req.Search, req.RestaurantNames);
//            return Ok(result);
//        }

//        // API tóm tắt review
//        [HttpPost("summarize")]
//        public async Task<IActionResult> Summarize([FromBody] SummarizeRequest req)
//        {
//            var result = await _openAIService.SummarizeReviewsAsync(req.RestaurantName, req.Reviews);
//            return Ok(result);
//        }
//    }

//    public class RecommendRequest
//    {
//        public string UserHistory { get; set; }
//        public string Search { get; set; }
//        public List<string> RestaurantNames { get; set; }
//    }

//    public class SummarizeRequest
//    {
//        public string RestaurantName { get; set; }
//        public List<string> Reviews { get; set; }
//    }
//}
