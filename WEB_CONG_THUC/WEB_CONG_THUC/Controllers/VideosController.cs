//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Hosting;
//using WEB_CONG_THUC.Models;

//using System.Text.RegularExpressions;
//using WEB_CONG_THUC.Repositories;
//using WEB_CONG_THUC.ViewModels;

//namespace CookShare.Controllers
//{
//    public class VideosController : Controller
//    {
//        private readonly IVideoRepository _videoRepository;
//        private readonly ICategoryRepository _categoryRepository;
//        private readonly IWebHostEnvironment _webHostEnvironment;

//        public VideosController(
//            IVideoRepository videoRepository,
//            ICategoryRepository categoryRepository,
//            IWebHostEnvironment webHostEnvironment)
//        {
//            _videoRepository = videoRepository;
//            _categoryRepository = categoryRepository;
//            _webHostEnvironment = webHostEnvironment;
//        }

//        // GET: Videos
//        public async Task<IActionResult> Index(int? categoryId, string searchString)
//        {
//            IEnumerable<Video> videos;

//            // Sử dụng repo để tìm kiếm video
//            if (!string.IsNullOrEmpty(searchString) || categoryId.HasValue)
//            {
//                videos = await _videoRepository.SearchVideosAsync(searchString, categoryId);
//            }
//            else
//            {
//                videos = await _videoRepository.GetApprovedVideosAsync();
//            }

//            var categories = await _categoryRepository.GetAllAsync();

//            var viewModel = new VideoIndexViewModel
//            {
//                Videos = videos.ToList(),
//                Categories = categories.ToList(),
//                SelectedCategoryId = categoryId,
//                SearchString = searchString
//            };

//            return View(viewModel);
//        }

//        // GET: Videos/Details/5
//        public async Task<IActionResult> Details(int id)
//        {
//            var video = await _videoRepository.GetByIdAsync(id);

//            if (video == null || !video.IsApproved)
//            {
//                return NotFound();
//            }

//            // Lấy các video liên quan (cùng danh mục)
//            IEnumerable<Video> relatedVideos;
//            if (video.CategoryId.HasValue)
//            {
//                relatedVideos = await _videoRepository.GetVideosByCategoryAsync(video.CategoryId.Value);
//                relatedVideos = relatedVideos.Where(v => v.Id != video.Id).Take(4);
//            }
//            else
//            {
//                relatedVideos = await _videoRepository.GetLatestVideosAsync(4);
//                relatedVideos = relatedVideos.Where(v => v.Id != video.Id);
//            }

//            var viewModel = new VideoDetailsViewModel
//            {
//                Video = video,
//                RelatedVideos = relatedVideos.ToList()
//            };

//            return View(viewModel);
//        }

//        // GET: Videos/Create
//        public async Task<IActionResult> Create()
//        {
//            var categories = await _categoryRepository.GetAllAsync();
//            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name");
//            return View();
//        }

//        // POST: Videos/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(VideoCreateViewModel viewModel)
//        {
//            var categories = await _categoryRepository.GetAllAsync();
//            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", viewModel.CategoryId);

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    // Xử lý URL video (lấy ID từ YouTube, Vimeo, v.v.)
//                    string videoUrl = ProcessVideoUrl(viewModel.VideoUrl);

//                    // Xử lý tải lên hình thu nhỏ
//                    string thumbnailUrl = null;
//                    if (viewModel.ThumbnailFile != null && viewModel.ThumbnailFile.Length > 0)
//                    {
//                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "thumbnails");

//                        // Tạo thư mục nếu chưa tồn tại
//                        if (!Directory.Exists(uploadsFolder))
//                        {
//                            Directory.CreateDirectory(uploadsFolder);
//                        }

//                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ThumbnailFile.FileName;
//                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

//                        using (var fileStream = new FileStream(filePath, FileMode.Create))
//                        {
//                            await viewModel.ThumbnailFile.CopyToAsync(fileStream);
//                        }

//                        thumbnailUrl = "/images/thumbnails/" + uniqueFileName;
//                    }
//                    else
//                    {
//                        // Nếu không có hình thu nhỏ, tạo hình thu nhỏ từ video (nếu là YouTube)
//                        if (videoUrl.Contains("youtube.com") || videoUrl.Contains("youtu.be"))
//                        {
//                            string videoId = ExtractYouTubeVideoId(viewModel.VideoUrl);
//                            if (!string.IsNullOrEmpty(videoId))
//                            {
//                                thumbnailUrl = $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
//                            }
//                        }
//                    }

//                    // Tạo đối tượng Video từ ViewModel
//                    var video = new Video
//                    {
//                        Title = viewModel.Title,
//                        Description = viewModel.Description,
//                        VideoUrl = videoUrl,
//                        ThumbnailUrl = thumbnailUrl,
//                        CategoryId = viewModel.CategoryId,
//                        AuthorName = viewModel.AuthorName ?? "Khách",
//                        AuthorEmail = viewModel.AuthorEmail,
//                        CreatedAt = DateTime.Now,
//                        IsApproved = false // Video cần được phê duyệt trước khi hiển thị
//                    };

//                    await _videoRepository.AddAsync(video);
//                    await _videoRepository.SaveChangesAsync();

//                    TempData["SuccessMessage"] = "Video của bạn đã được gửi và đang chờ phê duyệt.";
//                    return RedirectToAction(nameof(Index));
//                }
//                catch (Exception ex)
//                {
//                    ModelState.AddModelError("", "Lỗi khi lưu video: " + ex.Message);
//                }
//            }

//            return View(viewModel);
//        }

//        // GET: Videos/Admin
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Admin()
//        {
//            var pendingVideos = await _videoRepository.GetPendingVideosAsync();
//            var approvedVideos = await _videoRepository.GetApprovedVideosAsync();

//            var viewModel = new VideoAdminViewModel
//            {
//                PendingVideos = pendingVideos.ToList(),
//                ApprovedVideos = approvedVideos.ToList()
//            };

//            return View(viewModel);
//        }

//        // POST: Videos/Approve/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Approve(int id)
//        {
//            var video = await _videoRepository.GetByIdAsync(id);

//            if (video == null)
//            {
//                return NotFound();
//            }

//            video.IsApproved = true;
//            await _videoRepository.UpdateAsync(video);
//            await _videoRepository.SaveChangesAsync();

//            return RedirectToAction(nameof(Admin));
//        }

//        // POST: Videos/Reject/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Reject(int id)
//        {
//            var video = await _videoRepository.GetByIdAsync(id);

//            if (video == null)
//            {
//                return NotFound();
//            }

//            // Xóa hình thu nhỏ nếu có
//            if (!string.IsNullOrEmpty(video.ThumbnailUrl) && !video.ThumbnailUrl.StartsWith("http"))
//            {
//                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, video.ThumbnailUrl.TrimStart('/'));
//                if (System.IO.File.Exists(filePath))
//                {
//                    System.IO.File.Delete(filePath);
//                }
//            }

//            await _videoRepository.RemoveAsync(video);
//            await _videoRepository.SaveChangesAsync();

//            return RedirectToAction(nameof(Admin));
//        }

//        // Phương thức hỗ trợ xử lý URL video
//        private string ProcessVideoUrl(string url)
//        {
//            // Xử lý URL YouTube
//            if (url.Contains("youtube.com") || url.Contains("youtu.be"))
//            {
//                string videoId = ExtractYouTubeVideoId(url);
//                if (!string.IsNullOrEmpty(videoId))
//                {
//                    return $"https://www.youtube.com/embed/{videoId}";
//                }
//            }

//            // Xử lý URL Vimeo
//            if (url.Contains("vimeo.com"))
//            {
//                string videoId = ExtractVimeoVideoId(url);
//                if (!string.IsNullOrEmpty(videoId))
//                {
//                    return $"https://player.vimeo.com/video/{videoId}";
//                }
//            }

//            // Trả về URL gốc nếu không phải YouTube hoặc Vimeo
//            return url;
//        }

//        private string ExtractYouTubeVideoId(string url)
//        {
//            // Xử lý URL dạng youtube.com/watch?v=VIDEO_ID
//            var youtubeRegex = new Regex(@"(?:youtube\.com\/watch\?v=|youtu\.be\/)([^&\n?#]+)");
//            var match = youtubeRegex.Match(url);

//            if (match.Success)
//            {
//                return match.Groups[1].Value;
//            }

//            return null;
//        }

//        private string ExtractVimeoVideoId(string url)
//        {
//            // Xử lý URL dạng vimeo.com/VIDEO_ID
//            var vimeoRegex = new Regex(@"vimeo\.com\/(\d+)");
//            var match = vimeoRegex.Match(url);

//            if (match.Success)
//            {
//                return match.Groups[1].Value;
//            }

//            return null;
//        }
//    }
//}
