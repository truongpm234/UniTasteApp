using Microsoft.AspNetCore.Mvc;
using UserService.API.Service;
using UserService.API.Repository;
using UserService.API.Models.Entity;
using UserService.API.Models.DTO;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IGeminiAIService _geminiAIService;

        public AIController(IGeminiAIService geminiAIService)
        {
            _geminiAIService = geminiAIService;
        }
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromQuery] string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                return BadRequest("Prompt is required.");
            }
            try
            {
                var response = await _geminiAIService.getChatResponse(prompt);
                return Ok(new ChatResponse { Response = response});
            }
            catch (Exception)
            {

                throw;
            }
            
        }   
    }
}
