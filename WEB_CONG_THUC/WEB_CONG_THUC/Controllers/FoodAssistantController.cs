using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEB_CONG_THUC.Models; // Đảm bảo ChatMessage và RecipeInfo ở đây
using WEB_CONG_THUC.Services; // Đảm bảo GeminiFoodAssistantService ở đây

namespace WEB_CONG_THUC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodAssistantController : ControllerBase
    {
        private readonly GeminiFoodAssistantService _foodAssistantService;
        // Giả sử chúng ta có một nơi lưu trữ lịch sử chat tạm thời cho mỗi session/user
        // Trong thực tế, bạn có thể muốn dùng distributed cache hoặc DB.
        private static readonly Dictionary<string, List<ChatMessage>> _conversationHistories = new Dictionary<string, List<ChatMessage>>();

        public FoodAssistantController(GeminiFoodAssistantService foodAssistantService)
        {
            _foodAssistantService = foodAssistantService;
        }

        // POST api/FoodAssistant/suggest-food
        [HttpPost("suggest-food")]
        public async Task<IActionResult> SuggestFood([FromBody] FoodSuggestionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserInput))
            {
                return BadRequest("User input is required.");
            }

            // Lấy hoặc tạo lịch sử chat cho user/session
            // Đơn giản hóa: dùng sessionId từ request, hoặc tạo mới nếu không có
            var sessionId = string.IsNullOrWhiteSpace(request.SessionId) ? System.Guid.NewGuid().ToString() : request.SessionId;
            if (!_conversationHistories.TryGetValue(sessionId, out var history))
            {
                history = new List<ChatMessage>();
                _conversationHistories[sessionId] = history;
            }

            var suggestion = await _foodAssistantService.GetInteractiveFoodSuggestionAsync(request.UserInput, history);

            // Trả về cả suggestion và sessionId để client có thể tiếp tục cuộc hội thoại
            return Ok(new { SessionId = sessionId, Suggestion = suggestion });
        }

        // POST api/FoodAssistant/get-transcript
        [HttpPost("get-transcript")]
        public async Task<IActionResult> GetTranscript([FromBody] VideoUrlRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.VideoUrl))
            {
                return BadRequest("Video URL is required.");
            }

            // Hiện tại chỉ hỗ trợ YouTube
            if (!request.VideoUrl.Contains("youtube.com") && !request.VideoUrl.Contains("youtu.be"))
            {
                return BadRequest("Chỉ hỗ trợ lấy transcript từ URL YouTube ở thời điểm hiện tại.");
            }

            var transcript = await _foodAssistantService.GetTranscriptFromYouTubeUrlAsync(request.VideoUrl);

            if (transcript == null || transcript.StartsWith("Lỗi khi lấy transcript") || transcript == "Không tìm thấy phụ đề cho video này.")
            {
                return NotFound(transcript ?? "Không thể lấy transcript từ URL được cung cấp.");
            }

            return Ok(new { Transcript = transcript });
        }

        // POST api/FoodAssistant/extract-recipe
        [HttpPost("extract-recipe")]
        public async Task<IActionResult> ExtractRecipe([FromBody] ExtractRecipeRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.VideoTranscript))
            {
                return BadRequest("Video transcript is required.");
            }

            var recipeInfo = await _foodAssistantService.ExtractRecipeFromVideoTranscriptAsync(request.VideoTranscript);

            if (recipeInfo == null)
            {
                return NotFound("Could not extract recipe information from the provided transcript.");
            }

            return Ok(recipeInfo);
        }
    }

    // DTOs cho request bodies
    public class FoodSuggestionRequest
    {
        public string? SessionId { get; set; } // Để duy trì ngữ cảnh hội thoại
        public string UserInput { get; set; } = string.Empty;
    }

    public class ExtractRecipeRequest
    {
        public string VideoTranscript { get; set; } = string.Empty;
    }

    public class VideoUrlRequest // DTO mới cho request URL video
    {
        public string VideoUrl { get; set; } = string.Empty;
    }
}