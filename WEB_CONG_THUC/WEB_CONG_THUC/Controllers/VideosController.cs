using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WEB_CONG_THUC.Data;
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
        private readonly ApplicationDbContext _context;


        public VideosController(
            IVideoRepository videoRepository,
            IRecipeRepository recipeRepository,
            IWebHostEnvironment webHostEnvironment,
            UserManager<IdentityUser> userManager,
            ILogger<VideosController> logger,
            ApplicationDbContext context
            )
        {
            _videoRepository = videoRepository;
            _recipeRepository = recipeRepository;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _logger = logger;
            _context = context;

        }

        public async Task<IActionResult> Index(int? recipeId, string searchString)
        {
            try
            {
                // Lấy danh sách video đã được duyệt (Approved)
                var videosQuery = recipeId.HasValue
                    ? await _videoRepository.GetVideosByRecipeAsync(recipeId.Value)
                    : await _videoRepository.GetAllAsync();

                // Lọc chỉ lấy video đã được duyệt
                videosQuery = videosQuery.Where(v => v.Status == VideoStatus.Approved);

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
                    videosQuery = videosQuery.Where(v =>
                        v.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        v.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase));
                }

                // Lấy danh sách video yêu thích của người dùng hiện tại
                if (User.Identity!.IsAuthenticated)
                {
                    var userId = _userManager.GetUserId(User);
                    var favorites = await _videoRepository.GetFavoritesByUserIdAsync(userId!);
                    ViewBag.UserFavorites = favorites
                        .Where(v => v.Status == VideoStatus.Approved) // Chỉ lấy video đã duyệt
                        .Select(v => v.Id)
                        .ToList();
                }

                // Thêm thông báo nếu không có video nào
                if (!videosQuery.Any())
                {
                    ViewBag.Message = "Không có video nào được tìm thấy.";
                }

                return View(videosQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Index: {ex.Message}");
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {               
                var video = await _context.Videos
                    .Include(v => v.Recipe)
                    .Include(v => v.Comments)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(v => v.Id == id);
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
        public async Task<IActionResult> AddComment(int videoId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest();
            }

            var userId = _userManager.GetUserId(User);

            var comment = new VideoComment
            {
                VideoId = videoId,
                UserId = userId!,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.VideoComments.Add(comment);
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang Details của video hiện tại
            return RedirectToAction(nameof(Details), new { id = videoId });
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAllSlugs()
        {
            var videos = await _context.Videos.ToListAsync();
            foreach (var video in videos)
            {
                if (string.IsNullOrEmpty(video.Slug))
                {
                    video.Slug = SlugHelper.GenerateSlug(video.Title);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult Create()
        {
            return View(new VideoCreateViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VideoCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var video = new Video
                {
                    Title = model.Title,
                    Description = model.Description,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                    CreatedAt = DateTime.Now,
                    Status = VideoStatus.Pending,
                    Slug = SlugHelper.GenerateSlug(model.Title),
                    UploadType = model.UploadType
                };

                if (model.UploadType == VideoUploadType.Url)
                {
                    video.VideoUrl = model.VideoUrl ?? "";
                    if (!string.IsNullOrEmpty(model.VideoUrl))
                    {
                        string videoId = GetYouTubeVideoId(model.VideoUrl);
                        if (!string.IsNullOrEmpty(videoId))
                        {
                            video.ThumbnailUrl = $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
                        }
                    }
                }
                else if (model.VideoFile != null)
                {
                    string videoFileName = await SaveVideoFile(model.VideoFile);
                    video.VideoUrl = "/videos/" + videoFileName;
                }

                if (model.ThumbnailFile != null)
                {
                    string thumbnailFileName = await SaveThumbnailFile(model.ThumbnailFile);
                    video.ThumbnailUrl = "/images/videos/" + thumbnailFileName;
                }

                await _videoRepository.AddAsync(video);

                // Kiểm tra role và điều hướng
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Manage");
                }
                else
                {
                    // Lưu thông báo vào TempData
                    TempData["SubmissionMessage"] = "Bài đăng của bạn đã được gửi. Chờ một chút để chúng tôi xem xét và duyệt bài";
                    return RedirectToAction("SubmissionSuccess");
                }
            }
            return View(model);
        }

        // Thêm action mới để hiển thị trang thông báo thành công
        public IActionResult SubmissionSuccess()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage(string status = "All")
        {
            var videosQuery = _context.Videos
                .Include(v => v.User)
                .OrderByDescending(v => v.CreatedAt)
                .AsQueryable();

            if (Enum.TryParse<VideoStatus>(status, out var videoStatus) && status != "All")
            {
                videosQuery = videosQuery.Where(v => v.Status == videoStatus);
            }

            var videos = await videosQuery.ToListAsync();
            ViewBag.FilterStatus = status;
            return View(videos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null) return NotFound();

            // Để sửa đoạn này, bạn chỉ cần gán trạng thái video thành trạng thái đã duyệt (Approved) như sau:
            video.Status = VideoStatus.Approved;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null) return NotFound();

            video.Status = VideoStatus.Rejected;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }

        [Authorize]
        public async Task<IActionResult> MyVideos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var videos = await _context.Videos
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
            return View(videos);
        }

        private string GetYouTubeVideoId(string url)
        {
            if (url.Contains("youtube.com/watch?v="))
            {
                return url.Split("v=")[1].Split('&')[0];
            }
            else if (url.Contains("youtu.be/"))
            {
                return url.Split('/').Last().Split('?')[0];
            }
            return string.Empty;
        }

        private async Task<string> SaveVideoFile(IFormFile file)
        {
            // Tạo tên file độc nhất
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Đảm bảo thư mục tồn tại
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "videos");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Đường dẫn đầy đủ đến file
            string filePath = Path.Combine(uploadsFolder, fileName);

            // Lưu file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return fileName;
        }

        private async Task<string> SaveThumbnailFile(IFormFile file)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "videos", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Preview(int id)
        {
            var video = await _context.Videos
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
            {
                return NotFound();
            }

            return View(video);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var video = await _context.Videos
                .Include(v => v.Comments)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
                return NotFound();

            // Xóa file video nếu là video upload
            if (video.UploadType != VideoUploadType.Url && !string.IsNullOrEmpty(video.VideoUrl))
            {
                var videoPath = Path.Combine(_webHostEnvironment.WebRootPath, video.VideoUrl.TrimStart('/'));
                if (System.IO.File.Exists(videoPath))
                {
                    System.IO.File.Delete(videoPath);
                }
            }

            // Xóa thumbnail nếu có
            if (!string.IsNullOrEmpty(video.ThumbnailUrl) && !video.ThumbnailUrl.StartsWith("http"))
            {
                var thumbnailPath = Path.Combine(_webHostEnvironment.WebRootPath, video.ThumbnailUrl.TrimStart('/'));
                if (System.IO.File.Exists(thumbnailPath))
                {
                    System.IO.File.Delete(thumbnailPath);
                }
            }

            // Xóa các comments liên quan
            _context.VideoComments.RemoveRange(video.Comments);

            // Xóa video
            _context.Videos.Remove(video);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Video đã được xóa thành công.";
            return RedirectToAction(nameof(Manage));
        }
    }
}
