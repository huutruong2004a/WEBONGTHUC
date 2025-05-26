using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using System.Security.Claims;
using WEB_CONG_THUC.Models;
using WEB_CONG_THUC.Repositories;

namespace WEB_CONG_THUC.Controllers
{
    public class VideosController : Controller
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<VideosController> _logger;


        public VideosController(
            IVideoRepository videoRepository,
            IRecipeRepository recipeRepository,
            IWebHostEnvironment webHostEnvironment,
            UserManager<IdentityUser> userManager,
            ILogger<VideosController> logger
            )
        {
            _videoRepository = videoRepository;
            _recipeRepository = recipeRepository;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _logger = logger;

        }

        public async Task<IActionResult> Index(int? recipeId, string searchString)
        {
            try
            {
                // Lấy danh sách video
                var videos = recipeId.HasValue
                    ? await _videoRepository.GetVideosByRecipeAsync(recipeId.Value)
                    : await _videoRepository.GetAllAsync();

                // Lấy danh sách công thức cho dropdown
                var recipes = await _recipeRepository.GetAllAsync();
                ViewBag.Recipes = new SelectList(recipes, "Id", "Title", recipeId);

                // Lấy thông tin công thức hiện tại nếu có
                if (recipeId.HasValue)
                {
                    var recipe = await _recipeRepository.GetByIdAsync(recipeId.Value);
                    ViewBag.CurrentRecipe = recipe;
                }

                // Lọc theo từ khóa tìm kiếm nếu có
                if (!string.IsNullOrEmpty(searchString))
                {
                    videos = videos.Where(v =>
                        v.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        v.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase));
                }

                // Lấy danh sách video yêu thích của người dùng hiện tại
                if (User.Identity!.IsAuthenticated)
                {
                    var userId = _userManager.GetUserId(User);
                    var favorites = await _videoRepository.GetFavoritesByUserIdAsync(userId!);
                    ViewBag.UserFavorites = favorites.Select(v => v.Id).ToList();
                }

                return View(videos);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                _logger.LogError($"Error in Index: {ex.Message}");
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var video = await _videoRepository.GetByIdAsync(id);
                if (video == null)
                {
                    return NotFound();
                }

                // Tăng lượt xem
                video.ViewCount++;
                await _videoRepository.UpdateAsync(video);

                // Kiểm tra xem người dùng đã yêu thích video này chưa
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = _userManager.GetUserId(User);
                    var favorites = await _videoRepository.GetFavoritesByUserIdAsync(userId!);
                    ViewBag.IsFavorited = favorites.Any(v => v.Id == id);
                }

                // Lấy các video liên quan (cùng công thức hoặc cùng danh mục)
                if (video.RecipeId.HasValue)
                {
                    var relatedVideos = await _videoRepository.GetVideosByRecipeAsync(video.RecipeId.Value);
                    ViewBag.RelatedVideos = relatedVideos.Where(v => v.Id != id).Take(5);
                }

                return View(video);
            }
            catch (Exception ex)
            {
                // Log lỗi
                _logger.LogError($"Error in Details: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int videoId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _videoRepository.ToggleFavoriteAsync(videoId, userId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ToggleFavorite: {ex.Message}");
                return Json(new { success = false, error = "Có lỗi xảy ra" });
            }
        }

        [Authorize]
        public async Task<IActionResult> MyFavorites()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var videos = await _videoRepository.GetFavoritesByUserIdAsync(userId);
                return View(videos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MyFavorites: {ex.Message}");
                return View("Error");
            }
        }
    }
}
