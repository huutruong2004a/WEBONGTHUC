using GenerativeAI;
using GenerativeAI.Types;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WEB_CONG_THUC.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.ClosedCaptions;

namespace WEB_CONG_THUC.Services
{
    public class GeminiFoodAssistantService
    {
        private readonly GenerativeModel _geminiModel;
        private readonly IConfiguration _configuration;

        private const string GeminiFlashModel = "gemini-1.5-flash-latest";
        private const string UserRole = "user";
        private const string ModelRole = "model";

        public GeminiFoodAssistantService(IConfiguration configuration)
        {
            _configuration = configuration;
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("ERROR: Gemini API Key is not configured.");
                throw new ArgumentNullException(nameof(apiKey), "Gemini API Key is not configured.");
            }
            _geminiModel = new GenerativeModel(apiKey, model: GeminiFlashModel);
        }

        private string GetSystemFoodSuggestionPromptText()
        {
            return """
            Bạn là một chuyên gia ẩm thực AI. Hãy đưa ra những gợi ý món ăn thông minh và phù hợp. Khi người dùng hỏi, hãy xem xét các yếu tố như:
            - Sở thích cá nhân (nếu được cung cấp trong lịch sử chat).
            - Thời tiết hiện tại (nếu người dùng đề cập).
            - Địa điểm (nếu người dùng đề cập).
            - Thời điểm trong ngày (sáng, trưa, chiều, tối).
            - Các yêu cầu đặc biệt (ví dụ: món chay, ít calo, nguyên liệu cụ thể).
            Hãy sáng tạo và đưa ra các lựa chọn đa dạng. Nếu cần thêm thông tin để gợi ý tốt hơn, hãy đặt câu hỏi cho người dùng.
            """;
        }

        public async Task<string> GetInteractiveFoodSuggestionAsync(string userInput, IList<ChatMessage> conversationHistory)
        {
            if (conversationHistory == null)
            {
                conversationHistory = new List<ChatMessage>();
            }

            conversationHistory.Add(new ChatMessage(UserRole, userInput));

            var historyForChatSession = new List<Content>();
            historyForChatSession.Add(new Content { Role = UserRole, Parts = new List<Part> { new Part { Text = GetSystemFoodSuggestionPromptText() } } });

            for (int i = 0; i < conversationHistory.Count - 1; i++)
            {
                var msg = conversationHistory[i];
                historyForChatSession.Add(new Content { Role = msg.Role, Parts = new List<Part> { new Part { Text = msg.Content } } });
            }

            var chat = _geminiModel.StartChat(history: historyForChatSession);

            try
            {
                GenerateContentResponse result = await chat.GenerateContentAsync(userInput);

                if (result.Candidates != null && result.Candidates.Any())
                {
                    var modelResponseText = result.Text();
                    if (!string.IsNullOrEmpty(modelResponseText))
                    {
                        conversationHistory.Add(new ChatMessage(ModelRole, modelResponseText));
                        return modelResponseText;
                    }
                }
                conversationHistory.RemoveAt(conversationHistory.Count - 1); // Xóa userInput nếu không có phản hồi
                return "Xin lỗi, tôi không thể đưa ra gợi ý ngay lúc này.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Gemini API for suggestion: {ex.Message}");
                if (conversationHistory.Any() && conversationHistory.Last().Role == UserRole && conversationHistory.Last().Content == userInput)
                {
                    conversationHistory.RemoveAt(conversationHistory.Count - 1);
                }
                return "Đã có lỗi xảy ra khi cố gắng lấy gợi ý. Vui lòng thử lại sau.";
            }
        }

        public async Task<RecipeInfo?> ExtractRecipeFromVideoTranscriptAsync(string videoTranscript)
        {
            Console.WriteLine("LOG: ExtractRecipeFromVideoTranscriptAsync called.");

            if (string.IsNullOrWhiteSpace(videoTranscript))
            {
                Console.WriteLine("LOG: Video transcript is null or whitespace.");
                return null;
            }

            Console.WriteLine($"LOG: Input transcript (first 500 chars): {videoTranscript.Substring(0, Math.Min(videoTranscript.Length, 500))}");

            const int maxTranscriptLength = 30000;
            if (videoTranscript.Length > maxTranscriptLength)
            {
                videoTranscript = videoTranscript.Substring(0, maxTranscriptLength);
                Console.WriteLine($"LOG: Transcript truncated to {maxTranscriptLength} characters.");
            }

            var prompt = $"""
            Phân tích nội dung bản ghi (transcript) của video hướng dẫn nấu ăn sau đây và trích xuất thông tin chi tiết của công thức.
            Vui lòng trả lời CHỈ BẰNG MỘT CHUỖI JSON hợp lệ.
            Đối tượng JSON phải có các trường sau (nếu có thông tin, nếu không có thì để giá trị là null hoặc một chuỗi rỗng cho các trường không bắt buộc):
            - "DishName": tên món ăn (string, bắt buộc)
            - "Ingredients": danh sách các nguyên liệu kèm số lượng và đơn vị (array of strings, bắt buộc)
            - "Instructions": các bước thực hiện món ăn, mô tả rõ ràng từng bước (array of strings, bắt buộc)
            - "PreparationTime": thời gian chuẩn bị (string, ví dụ: "20 phút")
            - "CookingTime": thời gian nấu (string, ví dụ: "1 giờ")
            - "Servings": số lượng khẩu phần (string, ví dụ: "4 người ăn")

            Nội dung bản ghi video:
            ---
            {videoTranscript}
            ---
            JSON:
            """;
            Console.WriteLine("LOG: Prompt for Gemini prepared (transcript part omitted for brevity in console).");

            try
            {
                var generationConfig = new GenerationConfig
                {
                    ResponseMimeType = "application/json",
                };

                var request = new GenerateContentRequest();
                request.AddText(prompt);
                request.GenerationConfig = generationConfig;

                Console.WriteLine("LOG: Sending request to Gemini API for recipe extraction...");
                GenerateContentResponse result = await _geminiModel.GenerateContentAsync(request);

                if (result.Candidates != null && result.Candidates.Any())
                {
                    var jsonString = result.Text();
                    Console.WriteLine($"LOG: Raw JSON string received from Gemini: {jsonString}");

                    if (!string.IsNullOrWhiteSpace(jsonString))
                    {
                        if (jsonString.StartsWith("```json"))
                        {
                            jsonString = jsonString.Substring(7);
                            if (jsonString.EndsWith("```"))
                            {
                                jsonString = jsonString.Substring(0, jsonString.Length - 3);
                            }
                            Console.WriteLine($"LOG: JSON string after removing markdown: {jsonString}");
                        }
                        jsonString = jsonString.Trim();

                        try
                        {
                            var recipe = JsonSerializer.Deserialize<RecipeInfo>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            Console.WriteLine("LOG: JSON deserialized successfully.");
                            if (recipe != null)
                            {
                                Console.WriteLine($"LOG: Recipe DishName: {recipe.DishName}");
                                Console.WriteLine($"LOG: Recipe Ingredients count: {recipe.Ingredients?.Count}");
                                Console.WriteLine($"LOG: Recipe Instructions count: {recipe.Instructions?.Count}");
                                Console.WriteLine($"LOG: Recipe PreparationTime: {recipe.PreparationTime}");
                                Console.WriteLine($"LOG: Recipe CookingTime: {recipe.CookingTime}");
                                Console.WriteLine($"LOG: Recipe Servings: {recipe.Servings}");
                            }
                            else
                            {
                                Console.WriteLine("LOG: Deserialized recipe object is null.");
                            }
                            return recipe;
                        }
                        catch (JsonException jsonEx)
                        {
                            Console.WriteLine($"ERROR: JSON Deserialization Error: {jsonEx.Message}. Received: {jsonString}");
                            return null;
                        }
                    }
                    else
                    {
                        Console.WriteLine("LOG: Received empty or whitespace JSON string from Gemini.");
                    }
                }
                else
                {
                    Console.WriteLine("LOG: No candidates received from Gemini API.");
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Error calling Gemini API for recipe extraction: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> GetTranscriptFromYouTubeUrlAsync(string videoUrl)
        {
            var videoIdAttempt = YoutubeExplode.Videos.VideoId.TryParse(videoUrl);
            if (string.IsNullOrWhiteSpace(videoUrl) || videoIdAttempt == null)
            {
                return "URL video không hợp lệ hoặc không được hỗ trợ.";
            }

            var youtube = new YoutubeClient();
            try
            {
                var videoId = videoIdAttempt.Value;
                var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

                if (trackManifest.Tracks.Any())
                {
                    var trackInfo = trackManifest.Tracks.FirstOrDefault(t => t.Language.Code.Equals("vi", StringComparison.OrdinalIgnoreCase)) ??
                                    trackManifest.Tracks.FirstOrDefault(t => t.Language.Code.Equals("en", StringComparison.OrdinalIgnoreCase)) ??
                                    trackManifest.Tracks.FirstOrDefault();

                    if (trackInfo != null)
                    {
                        var captions = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);
                        var transcript = string.Join("\n", captions.Captions.Select(c => c.Text));
                        return transcript;
                    }
                }
                return "Không tìm thấy phụ đề cho video này.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting transcript from YouTube: {ex.Message}");
                return $"Lỗi khi lấy transcript: {ex.Message}";
            }
        }
    }
}