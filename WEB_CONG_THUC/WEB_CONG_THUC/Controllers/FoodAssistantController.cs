using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEB_CONG_THUC.Models; // Đảm bảo ChatMessage và RecipeInfo ở đây
using WEB_CONG_THUC.Services; // Đảm bảo GeminiFoodAssistantService ở đây
using System.Text.RegularExpressions;

namespace WEB_CONG_THUC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodAssistantController : ControllerBase
    {
        private readonly GeminiFoodAssistantService _foodAssistantService;
        private static readonly Dictionary<string, List<ChatMessage>> _conversationHistories = new Dictionary<string, List<ChatMessage>>();

        public FoodAssistantController(GeminiFoodAssistantService foodAssistantService)
        {
            _foodAssistantService = foodAssistantService;
        }

        [HttpPost("process-message")]
        public async Task<IActionResult> ProcessMessage([FromBody] ChatMessageRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserInput))
            {
                return BadRequest("User input is required.");
            }

            var sessionId = string.IsNullOrWhiteSpace(request.SessionId) ? System.Guid.NewGuid().ToString() : request.SessionId;
            if (!_conversationHistories.TryGetValue(sessionId, out var history))
            {
                history = new List<ChatMessage>();
                _conversationHistories[sessionId] = history;
            }

            // Determine action based on ActionType or UserInput content
            string action = request.ActionType ?? DetermineActionFromInput(request.UserInput);

            object responseData;
            string responseType;

            switch (action)
            {
                case "extractRecipeViaUrl":
                    if (!IsValidYouTubeUrl(request.UserInput))
                    {
                        responseData = "Vui lòng cung cấp một URL YouTube hợp lệ để trích xuất công thức.";
                        responseType = "error";
                        break;
                    }
                    var recipeInfo = await _foodAssistantService.ExtractRecipeFromYouTubeVideoAsync(request.UserInput);
                    if (recipeInfo != null)
                    {
                        responseData = recipeInfo;
                        responseType = "recipeInfo";
                    }
                    else
                    {
                        responseData = "Không thể trích xuất công thức từ URL được cung cấp. Video có thể không chứa công thức rõ ràng, hoặc transcript không có sẵn/không phù hợp.";
                        responseType = "error";
                    }
                    break;

                case "getSuggestion":
                default: // Default to suggestion if action is unknown or not provided
                    var suggestion = await _foodAssistantService.GetInteractiveFoodSuggestionAsync(request.UserInput, history);
                    responseData = suggestion;
                    responseType = "suggestion";
                    break;
            }
            return Ok(new { SessionId = sessionId, ResponseType = responseType, Data = responseData });
        }

        private string DetermineActionFromInput(string userInput)
        {
            if (IsValidYouTubeUrl(userInput))
            {
                return "extractRecipeViaUrl";
            }
            return "getSuggestion";
        }

        private bool IsValidYouTubeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            // Simple regex for YouTube URLs (can be made more comprehensive)
            return Regex.IsMatch(url, @"^(https?:\/\/)?(www\.)?(youtube\.com\/watch\?v=|youtu\.be\/)([\w-]+)");
        }
    }

    public class ChatMessageRequest // New DTO for combined requests
    {
        public string? SessionId { get; set; }
        public string UserInput { get; set; } = string.Empty;
        public string? ActionType { get; set; } // e.g., "getSuggestion", "extractRecipeViaUrl"
    }

    // Removed FoodSuggestionRequest, ExtractRecipeRequest, VideoUrlRequest as they are replaced by ChatMessageRequest
}