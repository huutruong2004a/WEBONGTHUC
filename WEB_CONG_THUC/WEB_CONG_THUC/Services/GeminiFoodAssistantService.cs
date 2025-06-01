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
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

namespace WEB_CONG_THUC.Services
{
    public class GeminiFoodAssistantService
    {
        private readonly GenerativeModel _recipeExtractionModel; // Renamed from _geminiModel
        private readonly GenerativeModel _chatModel; // New model for chat
        private readonly IConfiguration _configuration;

        private const string RecipeExtractionModelName = "gemini-1.5-pro-latest"; // Was GeminiFlashModel, kept as pro for extraction
        private const string ChatModelName = "gemini-1.5-flash-latest"; // Reverted to 1.5-flash-latest for chat speed
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
            _recipeExtractionModel = new GenerativeModel(apiKey, model: RecipeExtractionModelName);
            _chatModel = new GenerativeModel(apiKey, model: ChatModelName); // Initialize chat model
        }

        private string GetSystemFoodSuggestionPromptText()
        {
            return """
            Bạn là một trợ lý ẩm thực AI thông thái và chủ động. Mục tiêu của bạn là đưa ra những gợi ý món ăn hấp dẫn và phù hợp một cách nhanh chóng và tự nhiên.

            Khi người dùng yêu cầu gợi ý, hãy:
            1.  Ưu tiên đưa ra 1-3 gợi ý cụ thể ngay lập tức. Dựa vào thông tin người dùng cung cấp (nếu có) và các yếu tố chung như thời điểm trong ngày (ví dụ: bữa sáng, trưa, tối), hoặc các món phổ biến.
            2.  Cố gắng suy đoán thông minh: Nếu người dùng nói "tôi đói", hãy gợi ý các món chính. Nếu họ nói "thèm gì đó ngọt", hãy gợi ý món tráng miệng.
            3.  Hạn chế hỏi nhiều câu hỏi ban đầu. Chỉ hỏi thêm thông tin nếu gợi ý ban đầu của bạn chưa phù hợp và người dùng muốn có lựa chọn khác, hoặc nếu yêu cầu của họ quá mơ hồ và bạn không thể đưa ra bất kỳ gợi ý hợp lý nào.
            4.  Khi hỏi, hãy hỏi một cách cụ thể để thu hẹp lựa chọn, ví dụ: "Bạn thích món có thịt hay món chay?", "Bạn muốn món nước hay món khô?".
            5.  Luôn xem xét lịch sử trò chuyện (nếu có) để hiểu sở thích hoặc các yêu cầu đã được đề cập trước đó của người dùng.
            6.  Nếu người dùng cung cấp các yếu tố như thời tiết, địa điểm, yêu cầu đặc biệt (món chay, ít calo, nguyên liệu cụ thể), hãy tích hợp chúng vào gợi ý của bạn.
            7.  Nếu người dùng cung cấp một video YouTube, hãy phân tích nội dung video đó để trích xuất công thức nấu ăn và đưa ra gợi ý dựa trên công thức đó.
            8.  **Quan trọng:** Hãy trả lời một cách tự nhiên, giống như một người bạn đang trò chuyện. Tránh sử dụng các định dạng đặc biệt như dấu sao để làm đậm chữ (ví dụ: **Bún Bò Huế**) hoặc đánh số thứ tự cho các gợi ý (ví dụ: 1., 2., 3.). Thay vào đó, hãy trình bày các gợi ý một cách liền mạch trong một đoạn văn hoặc các câu riêng biệt nếu cần.
            Hãy luôn sáng tạo, đưa ra các lựa chọn đa dạng và làm cho trải nghiệm tìm kiếm món ăn trở nên thú vị và ít phiền hà nhất cho người dùng.
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

            var chat = _chatModel.StartChat(history: historyForChatSession); // Use _chatModel for suggestions

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

        // Các phương thức ExtractRecipeFromVideoTranscriptAsync và ExtractRecipeFromYouTubeVideoAsync
        // sẽ tự động sử dụng _recipeExtractionModel vì chúng đang gọi _geminiModel (đã được đổi tên thành _recipeExtractionModel).
        // Ví dụ trong ExtractRecipeFromVideoTranscriptAsync:
        // GenerateContentResponse result = await _recipeExtractionModel.GenerateContentAsync(request);
        // Và tương tự cho ExtractRecipeFromYouTubeVideoAsync.
        // Không cần thay đổi trực tiếp trong các phương thức đó nếu chúng đã dùng biến instance đúng.

        // Kiểm tra lại các lệnh gọi GenerateContentAsync trong ExtractRecipeFromVideoTranscriptAsync và ExtractRecipeFromYouTubeVideoAsync
        // để đảm bảo chúng sử dụng _recipeExtractionModel.
        // Hiện tại, chúng đang sử dụng _geminiModel, sẽ được đổi tên thành _recipeExtractionModel.
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

            string jsonExample = """
            {
              "DishName": "Gà Rang Muối",
              "Ingredients": ["Nửa con gà ta (khoảng 600 gram)", "1 gói bột chiên giòn", "2 quả trứng gà", "Sả", "Lá chanh", "Ớt", "Muối", "Đường", "Hạt nêm"],
              "Instructions": ["Gà chặt miếng vừa ăn.", "Ướp gà với chút muối, hạt nêm trong 15 phút.", "Sả băm nhỏ, lá chanh thái sợi.", "Trứng gà đánh tan.", "Nhúng gà qua trứng rồi lăn qua bột chiên giòn.", "Bắc chảo dầu nóng, chiên gà vàng giòn.", "Vớt gà ra để ráo dầu.", "Phi thơm sả.", "Cho gà đã chiên vào đảo nhanh tay với một ít muối rang và lá chanh."],
              "PreparationTime": "15 phút",
              "CookingTime": "30 phút",
              "Servings": null
            }
            """;

            var prompt = $"""
            Bạn là một AI chuyên nghiệp trong việc phân tích video hướng dẫn nấu ăn và trích xuất công thức chi tiết.
            Nhiệm vụ của bạn là phân tích nội dung bản ghi (transcript) của video hướng dẫn nấu ăn sau đây và trả về MỘT CHUỖI JSON HỢP LỆ DUY NHẤT.
            Đối tượng JSON phải tuân thủ cấu trúc sau. Đối với các trường array, nếu không có thông tin, hãy để là một array rỗng [].

            Cấu trúc JSON yêu cầu:
            - "DishName": Tên món ăn (string, bắt buộc và phải chính xác nhất có thể). Ví dụ: "Gà Kho Gừng".
            - "Ingredients": Danh sách các nguyên liệu (array of strings, bắt buộc và phải chính xác nhất có thể). Mỗi chuỗi trong array phải mô tả một nguyên liệu, bao gồm số lượng, đơn vị (nếu có), và tên nguyên liệu. Ví dụ: ["1 con gà (khoảng 1.2kg)", "50g gừng tươi", "2 củ hành tím", "3 muỗng canh nước mắm ngon", "1 muỗng cà phê đường", "1/2 muỗng cà phê tiêu xay", "Hành lá, ớt (tùy chọn)"].
            - "Instructions": Các bước thực hiện món ăn (array of strings, bắt buộc và phải chính xác nhất có thể). Mỗi chuỗi trong array là một bước hướng dẫn rõ ràng, theo trình tự. Ví dụ: ["Gà làm sạch, chặt miếng vừa ăn.", "Gừng cạo vỏ, một nửa băm nhỏ, một nửa thái sợi.", "Ướp gà với gừng băm, hành tím băm, nước mắm, đường, tiêu trong khoảng 20-30 phút.", "Phi thơm hành tím và gừng thái sợi với chút dầu ăn, sau đó cho gà đã ướp vào xào săn.", "Thêm một ít nước (hoặc nước dừa tươi) xâm xấp mặt thịt, đun lửa nhỏ cho đến khi gà mềm và nước sốt sánh lại.", "Nêm nếm lại gia vị cho vừa ăn, thêm hành lá, ớt (nếu dùng) rồi tắt bếp."].
            - "PreparationTime": Thời gian chuẩn bị (string). Cố gắng ước tính một giá trị hợp lý (ví dụ: "Khoảng 30 phút") nếu thông tin không được cung cấp rõ ràng trong video. Nếu hoàn toàn không thể ước tính, hãy để là một chuỗi rỗng.
            - "CookingTime": Thời gian nấu (string). Cố gắng ước tính một giá trị hợp lý (ví dụ: "Khoảng 1 giờ") nếu thông tin không được cung cấp rõ ràng trong video. Nếu hoàn toàn không thể ước tính, hãy để là một chuỗi rỗng.
            - "Servings": Số lượng khẩu phần (string). Cố gắng ước tính một giá trị hợp lý (ví dụ: "Cho 2-4 người") nếu thông tin không được cung cấp rõ ràng trong video. Nếu hoàn toàn không thể ước tính, hãy để là một chuỗi rỗng.

            Ví dụ cụ thể về cách trích xuất:

            Nội dung đầu vào mẫu:
            ---
            Chào các bạn, hôm nay mình sẽ chia sẻ cách làm món gà rang muối.
            Nguyên liệu gồm có nửa con gà ta, khoảng 600 gram. Một gói bột chiên giòn, hai quả trứng gà. Sả, lá chanh, ớt. Gia vị thì có muối, đường, hạt nêm.
            Đầu tiên, gà chặt miếng vừa ăn. Ướp gà với chút muối, hạt nêm trong 15 phút.
            Sả thì băm nhỏ, lá chanh thái sợi.
            Trứng gà đánh tan. Nhúng gà qua trứng rồi lăn qua bột chiên giòn.
            Bắc chảo dầu nóng, chiên gà vàng giòn. Vớt gà ra để ráo dầu.
            Phi thơm sả, cho gà đã chiên vào đảo nhanh tay với một ít muối rang và lá chanh. Xong rồi! Món này ăn nóng rất ngon.
            Thời gian nấu chắc tầm 30 phút thôi. Chuẩn bị thì 15 phút ướp gà.
            ---
            JSON kết quả mẫu:
            {jsonExample}

            Bây giờ, hãy phân tích nội dung bản ghi video sau đây:

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
                GenerateContentResponse result = await _recipeExtractionModel.GenerateContentAsync(request); // Use _recipeExtractionModel

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
                                // Post-processing: Provide estimated values if fields are empty or null
                                if (string.IsNullOrWhiteSpace(recipe.PreparationTime))
                                {
                                    recipe.PreparationTime = "Khoảng 10-20 phút (ước tính)";
                                }
                                if (string.IsNullOrWhiteSpace(recipe.CookingTime))
                                {
                                    recipe.CookingTime = "Khoảng 20-40 phút (ước tính)";
                                }
                                if (string.IsNullOrWhiteSpace(recipe.Servings))
                                {
                                    recipe.Servings = "Cho 2-3 người (ước tính)";
                                }

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

        public async Task<RecipeInfo?> ExtractRecipeFromYouTubeVideoAsync(string videoUrl)
        {
            Console.WriteLine($"LOG: ExtractRecipeFromYouTubeVideoAsync called with URL: {videoUrl}");

            var videoData = await GetYouTubeVideoDataAsync(videoUrl);

            if (videoData.HasError || string.IsNullOrWhiteSpace(videoData.Transcript))
            {
                Console.WriteLine($"LOG: Could not retrieve sufficient video data or transcript. Error: {videoData.ErrorMessage}");
                return null;
            }

            // Combine title, description, and transcript for a richer prompt
            var combinedText = $"Tiêu đề video: {videoData.Title}\n\nMô tả video: {videoData.Description}\n\nNội dung bản ghi (transcript):\n{videoData.Transcript}";

            Console.WriteLine($"LOG: Combined text for prompt (first 500 chars): {combinedText.Substring(0, Math.Min(combinedText.Length, 500))}");

            const int maxInputLength = 30000; // Max length for the combined text
            if (combinedText.Length > maxInputLength)
            {
                combinedText = combinedText.Substring(0, maxInputLength);
                Console.WriteLine($"LOG: Combined text truncated to {maxInputLength} characters.");
            }

            string jsonExampleYouTube = """
            {
              "DishName": "Gà Rang Muối",
              "Ingredients": ["Nửa con gà ta (khoảng 600 gram)", "1 gói bột chiên giòn", "2 quả trứng gà", "Sả", "Lá chanh", "Ớt", "Muối", "Đường", "Hạt nêm"],
              "Instructions": ["Gà chặt miếng vừa ăn.", "Ướp gà với chút muối, hạt nêm trong 15 phút.", "Sả băm nhỏ, lá chanh thái sợi.", "Trứng gà đánh tan.", "Nhúng gà qua trứng rồi lăn qua bột chiên giòn.", "Bắc chảo dầu nóng, chiên gà vàng giòn.", "Vớt gà ra để ráo dầu.", "Phi thơm sả.", "Cho gà đã chiên vào đảo nhanh tay với một ít muối rang và lá chanh."],
              "PreparationTime": "15 phút",
              "CookingTime": "30 phút",
              "Servings": null
            }
            """;

            var prompt = $"""
            Bạn là một AI chuyên nghiệp trong việc phân tích video hướng dẫn nấu ăn và trích xuất công thức chi tiết.
            Nhiệm vụ của bạn là phân tích nội dung được cung cấp (bao gồm tiêu đề, mô tả và bản ghi chi tiết của video) và trả về MỘT CHUỖI JSON HỢP LỆ DUY NHẤT.
            Đối tượng JSON phải tuân thủ cấu trúc sau. Đối với các trường array, nếu không có thông tin, hãy để là một array rỗng [].

            Cấu trúc JSON yêu cầu:
            - "DishName": Tên món ăn (string, bắt buộc và phải chính xác nhất có thể). Ví dụ: "Gà Kho Gừng".
            - "Ingredients": Danh sách các nguyên liệu (array of strings, bắt buộc và phải chính xác nhất có thể). Mỗi chuỗi trong array phải mô tả một nguyên liệu, bao gồm số lượng, đơn vị (nếu có), và tên nguyên liệu. Ví dụ: ["1 con gà (khoảng 1.2kg)", "50g gừng tươi", "2 củ hành tím", "3 muỗng canh nước mắm ngon", "1 muỗng cà phê đường", "1/2 muỗng cà phê tiêu xay", "Hành lá, ớt (tùy chọn)"].
            - "Instructions": Các bước thực hiện món ăn (array of strings, bắt buộc và phải chính xác nhất có thể). Mỗi chuỗi trong array là một bước hướng dẫn rõ ràng, theo trình tự. Ví dụ: ["Gà làm sạch, chặt miếng vừa ăn.", "Gừng cạo vỏ, một nửa băm nhỏ, một nửa thái sợi.", "Ướp gà với gừng băm, hành tím băm, nước mắm, đường, tiêu trong khoảng 20-30 phút.", "Phi thơm hành tím và gừng thái sợi với chút dầu ăn, sau đó cho gà đã ướp vào xào săn.", "Thêm một ít nước (hoặc nước dừa tươi) xâm xấp mặt thịt, đun lửa nhỏ cho đến khi gà mềm và nước sốt sánh lại.", "Nêm nếm lại gia vị cho vừa ăn, thêm hành lá, ớt (nếu dùng) rồi tắt bếp."].
            - "PreparationTime": Thời gian chuẩn bị (string). Cố gắng ước tính một giá trị hợp lý (ví dụ: "Khoảng 30 phút") nếu thông tin không được cung cấp rõ ràng trong video. Nếu hoàn toàn không thể ước tính, hãy để là một chuỗi rỗng.
            - "CookingTime": Thời gian nấu (string). Cố gắng ước tính một giá trị hợp lý (ví dụ: "Khoảng 1 giờ") nếu thông tin không được cung cấp rõ ràng trong video. Nếu hoàn toàn không thể ước tính, hãy để là một chuỗi rỗng.
            - "Servings": Số lượng khẩu phần (string). Cố gắng ước tính một giá trị hợp lý (ví dụ: "Cho 2-4 người") nếu thông tin không được cung cấp rõ ràng trong video. Nếu hoàn toàn không thể ước tính, hãy để là một chuỗi rỗng.

            Ví dụ cụ thể về cách trích xuất:

            Nội dung đầu vào mẫu:
            ---
            Tiêu đề video: Cách làm Gà Rang Muối Siêu Ngon Tại Nhà
            Mô tả video: Công thức gà rang muối đơn giản, giòn rụm, đậm đà. Nguyên liệu dễ tìm, các bước dễ làm.
            Nội dung bản ghi (transcript):
            Chào các bạn, hôm nay mình sẽ chia sẻ cách làm món gà rang muối.
            Nguyên liệu gồm có nửa con gà ta, khoảng 600 gram. Một gói bột chiên giòn, hai quả trứng gà. Sả, lá chanh, ớt. Gia vị thì có muối, đường, hạt nêm.
            Đầu tiên, gà chặt miếng vừa ăn. Ướp gà với chút muối, hạt nêm trong 15 phút.
            Sả thì băm nhỏ, lá chanh thái sợi.
            Trứng gà đánh tan. Nhúng gà qua trứng rồi lăn qua bột chiên giòn.
            Bắc chảo dầu nóng, chiên gà vàng giòn. Vớt ra để ráo dầu.
            Phi thơm sả, cho gà đã chiên vào đảo nhanh tay với một ít muối rang và lá chanh. Xong rồi! Món này ăn nóng rất ngon.
            Thời gian nấu chắc tầm 30 phút thôi. Chuẩn bị thì 15 phút ướp gà.
            ---
            JSON kết quả mẫu:
            {jsonExampleYouTube}

            Bây giờ, hãy phân tích nội dung video sau đây:

            Nội dung video:
            ---
            {combinedText}
            ---
            JSON:
            """;
            Console.WriteLine("LOG: Prompt for Gemini prepared (combined text part omitted for brevity in console).");

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
                GenerateContentResponse result = await _recipeExtractionModel.GenerateContentAsync(request); // Use _recipeExtractionModel

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
                                // Post-processing: Provide estimated values if fields are empty or null
                                if (string.IsNullOrWhiteSpace(recipe.PreparationTime))
                                {
                                    recipe.PreparationTime = "Khoảng 10-20 phút (ước tính)";
                                }
                                if (string.IsNullOrWhiteSpace(recipe.CookingTime))
                                {
                                    recipe.CookingTime = "Khoảng 20-40 phút (ước tính)";
                                }
                                if (string.IsNullOrWhiteSpace(recipe.Servings))
                                {
                                    recipe.Servings = "Cho 2-3 người (ước tính)";
                                }

                                Console.WriteLine($"LOG: Recipe DishName: {recipe.DishName}");
                                // Add more detailed logging if needed
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

        public async Task<YouTubeVideoData> GetYouTubeVideoDataAsync(string videoUrl)
        {
            var videoIdAttempt = YoutubeExplode.Videos.VideoId.TryParse(videoUrl);
            if (string.IsNullOrWhiteSpace(videoUrl) || videoIdAttempt == null)
            {
                return new YouTubeVideoData { ErrorMessage = "URL video không hợp lệ hoặc không được hỗ trợ." };
            }

            var youtube = new YoutubeClient();
            try
            {
                var videoId = videoIdAttempt.Value;
                YoutubeExplode.Videos.Video video = await youtube.Videos.GetAsync(videoId); // Explicitly qualify Video type
                var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

                string? transcript = null;
                if (trackManifest.Tracks.Any())
                {
                    var trackInfo = trackManifest.Tracks.FirstOrDefault(t => t.Language.Code.Equals("vi", StringComparison.OrdinalIgnoreCase)) ??
                                    trackManifest.Tracks.FirstOrDefault(t => t.Language.Code.Equals("en", StringComparison.OrdinalIgnoreCase)) ??
                                    trackManifest.Tracks.FirstOrDefault();

                    if (trackInfo != null)
                    {
                        var captions = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);
                        transcript = string.Join("\n", captions.Captions.Select(c => c.Text));
                    }
                }

                if (transcript == null)
                {
                    Console.WriteLine($"LOG: No transcript found for video: {videoUrl}. Title and Description will still be used.");
                    // Return data even if transcript is missing, the calling method can decide how to handle it.
                }

                return new YouTubeVideoData
                {
                    Title = video.Title,
                    Description = video.Description,
                    Transcript = transcript
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting YouTube video data: {ex.Message}");
                return new YouTubeVideoData { ErrorMessage = ex.Message };
            }
        }
    }
}