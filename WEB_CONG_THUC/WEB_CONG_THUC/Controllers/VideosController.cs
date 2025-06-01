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

        public async Task<IActionResult> Index(int? categoryId, string? searchString, int? recipeId) // Thêm recipeId nhưng không dùng nữa
        {
            try
            {
                var videosQuery = _context.Videos
                                        .Include(v => v.User)
                                        .Include(v => v.Recipe) // Vẫn include Recipe nếu bạn muốn hiển thị thông tin Recipe
                                        .Include(v => v.Category) // Include Category
                                        .Where(v => v.Status == VideoStatus.Approved)
                                        .AsQueryable();

                if (categoryId.HasValue)
                {
                    videosQuery = videosQuery.Where(v => v.CategoryId == categoryId.Value);
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    videosQuery = videosQuery.Where(v =>
                        v.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        (v.Description != null && v.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase)) || 
                        (v.User != null && v.User.UserName != null && v.User.UserName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                        (v.Category != null && v.Category.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))); // Tìm kiếm theo tên Category
                }

                // Lấy danh sách Categories cho dropdown
                var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
                ViewBag.SelectedCategoryId = categoryId;

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
                    .Include(v => v.User) 
                    .Include(v => v.Category) // Đảm bảo Category được tải
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
        public async Task<IActionResult> AddComment(int videoId, string? content, int rating, string returnUrl)
        {
            // Allow empty comment if rating is provided
            if (string.IsNullOrWhiteSpace(content) && rating == 0)
            {
                TempData["ErrorMessage"] = "Nội dung bình luận hoặc đánh giá không được để trống.";
                return Redirect(returnUrl ?? Url.Action("Details", new { id = videoId })!);
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge(); // Should not happen if [Authorize] is working
            }

            var comment = new VideoComment
            {
                VideoId = videoId,
                UserId = userId,
                Content = content ?? string.Empty, // Handle potentially null content
                Rating = rating,
                CreatedAt = DateTime.Now
            };

            _context.VideoComments.Add(comment); // Use _context to add comment
            await _context.SaveChangesAsync(); // Save changes to the database

            TempData["SuccessMessage"] = "Bình luận của bạn đã được gửi.";
            return Redirect(returnUrl ?? Url.Action("Details", new { id = videoId })!);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "Người dùng không được ủy quyền." });
                }

                if (request == null)
                {
                    _logger.LogWarning("ToggleFavorite được gọi với request null.");
                    return BadRequest(new { success = false, error = "Yêu cầu không hợp lệ." });
                }

                var result = await _videoRepository.ToggleFavoriteAsync(request.VideoId, userId);
                return Json(new { success = true, isFavorited = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong ToggleFavorite cho VideoId: {VideoId}", request?.VideoId);
                return Json(new { success = false, error = "Đã xảy ra lỗi khi thay đổi trạng thái yêu thích." });
            }
        }

        [Authorize]
        public async Task<IActionResult> MyFavoriteVideos() // Đổi tên từ MyFavorites
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var videos = await _videoRepository.GetFavoritesByUserIdAsync(userId);
                return View(videos); // View này sẽ là MyFavoriteVideos.cshtml
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MyFavoriteVideos: {ex.Message}");
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
        public async Task<IActionResult> Create() // Sửa GET Create thành async
        {
            var viewModel = new VideoCreateViewModel();
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VideoCreateViewModel model) // Sửa POST Create
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
                    UploadType = model.UploadType,
                    CategoryId = model.CategoryId // Lưu CategoryId
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
            // Nếu ModelState không hợp lệ, tải lại danh sách Categories
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", model.CategoryId);
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

            // Đường dẫn đến thư mục wwwroot gốc của dự án
            string sourceWwwRoot = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            string uploadsFolder = Path.Combine(sourceWwwRoot, "videos");

            // Đảm bảo thư mục tồn tại trong wwwroot gốc
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Đường dẫn đầy đủ đến file trong wwwroot gốc
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
            
            // Đường dẫn đến thư mục wwwroot gốc của dự án
            string sourceWwwRoot = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            string targetFolder = Path.Combine(sourceWwwRoot, "images", "videos");

            // Đảm bảo thư mục tồn tại trong wwwroot gốc
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string filePath = Path.Combine(targetFolder, fileName);

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

            string sourceWwwRoot = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");

            // Xóa file video nếu là video upload từ wwwroot gốc
            if (video.UploadType != VideoUploadType.Url && !string.IsNullOrEmpty(video.VideoUrl))
            {
                var videoPath = Path.Combine(sourceWwwRoot, video.VideoUrl.TrimStart('/'));
                if (System.IO.File.Exists(videoPath))
                {
                    System.IO.File.Delete(videoPath);
                }
            }

            // Xóa thumbnail nếu có từ wwwroot gốc
            if (!string.IsNullOrEmpty(video.ThumbnailUrl) && !video.ThumbnailUrl.StartsWith("http"))
            {
                var thumbnailPath = Path.Combine(sourceWwwRoot, video.ThumbnailUrl.TrimStart('/'));
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
