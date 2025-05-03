using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WEB_CONG_THUC.Models;
using WEB_CONG_THUC.Repositories;

namespace WEB_CONG_THUC.Controllers
{
    public class BlogController : Controller
    {
       
        private readonly IBlogRepository _blogRepository;

        public BlogController(IBlogRepository blogRepository)
        {
            _blogRepository = blogRepository;
        }


        public async Task<IActionResult> Index(int page = 1, string? searchTerm = null)
        {
            // Lấy danh sách các bài blog đã được duyệt
            var blogs = await _blogRepository.GetApprovedAsync();

            // Lấy top 3 bài blog có lượt xem cao nhất (không bị ảnh hưởng bởi tìm kiếm)
            var topViewedBlogs = blogs.OrderByDescending(b => b.ViewCount).Take(3).ToList();
            ViewBag.TopViewedBlogs = topViewedBlogs; // Truyền top 3 vào ViewBag

            // Chuyển sang IQueryable để có thể lọc và phân trang
            var blogQuery = blogs.AsQueryable();

            // Xử lý tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                blogQuery = blogQuery.Where(b => b.Title.ToLower().Contains(searchTerm) ||
                                                 System.Text.RegularExpressions.Regex.Replace(b.Content.ToLower(), "<[^>]+>", " ").Contains(searchTerm));
            }

            // Chuyển kết quả tìm kiếm thành danh sách để sử dụng nhiều lần
            var filteredBlogs = blogQuery.ToList();

            // Phân trang
            int pageSize = 6; // Số bài trên mỗi trang
            int totalItems = filteredBlogs.Count(); // Tổng số bài sau khi lọc
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize); // Tổng số trang

            // Đảm bảo page hợp lệ
            page = Math.Max(1, Math.Min(page, totalPages));

            // Lấy danh sách blog cho trang hiện tại
            var pagedBlogs = filteredBlogs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Truyền thông tin phân trang và tìm kiếm vào ViewBag
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;

            return View(pagedBlogs);
        }


        // Trang chi tiết bài đăng
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            var blog = await _blogRepository.GetBySlugAsync(slug);
            if (blog == null || blog.Status != BlogStatus.Approved)
                return NotFound();
            // Tăng số lượt xem
            blog.ViewCount += 1;
            await _blogRepository.UpdateAsync(blog);
            await _blogRepository.SaveAsync();

            return View(blog);
        }

        // Trang tạo bài đăng
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý tạo bài đăng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {
            if (ModelState.IsValid)
            {
                blog.Slug = SlugHelper.GenerateSlug(blog.Title);
                blog.CreatedAt = DateTime.Now;
                blog.Status = BlogStatus.Pending;

                await _blogRepository.AddAsync(blog);
                await _blogRepository.SaveAsync();

                return RedirectToAction("Index");
            }

            return View(blog);
        }

        // Xử lý upload ảnh
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var imageUrl = Url.Content("~/uploads/" + fileName);
                return Content(imageUrl);
            }

            return BadRequest();
        }

        // Trang quản lý bài đăng (cho admin)
        [HttpGet]
        public async Task<IActionResult> Manage(string status = "All")
        {
            var blogs = await _blogRepository.GetAllAsync();

            if (status != "All")
            {
                if (Enum.TryParse<BlogStatus>(status, out var blogStatus))
                {
                    blogs = blogs.Where(b => b.Status == blogStatus).ToList();
                }
            }

            ViewBag.FilterStatus = status;
            return View(blogs);
        }

        // Duyệt bài đăng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var blog = await _blogRepository.GetByIdAsync(id);
            if (blog == null)
                return NotFound();

            blog.Status = BlogStatus.Approved;
            await _blogRepository.UpdateAsync(blog);
            await _blogRepository.SaveAsync();

            return RedirectToAction("Manage");
        }

        // Từ chối bài đăng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var blog = await _blogRepository.GetByIdAsync(id);
            if (blog == null)
                return NotFound();

            blog.Status = BlogStatus.Rejected;
            await _blogRepository.UpdateAsync(blog);
            await _blogRepository.SaveAsync();

            return RedirectToAction("Manage");
        }

        // Trang chỉnh sửa bài đăng
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var blog = await _blogRepository.GetByIdAsync(id);
            if (blog == null)
                return NotFound();

            return View(blog);
        }

        // Xử lý chỉnh sửa bài đăng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Blog blog)
        {
            if (ModelState.IsValid)
            {
                var existingBlog = await _blogRepository.GetByIdAsync(blog.Id);
                if (existingBlog == null)
                    return NotFound();

                existingBlog.Title = blog.Title;
                existingBlog.Content = blog.Content;
                existingBlog.Slug = SlugHelper.GenerateSlug(blog.Title);
                existingBlog.Status = blog.Status; // Cho phép thay đổi trạng thái nếu cần

                await _blogRepository.UpdateAsync(existingBlog);
                await _blogRepository.SaveAsync();

                return RedirectToAction("Manage");
            }

            return View(blog);
        }

        // Xóa bài đăng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _blogRepository.GetByIdAsync(id);
            if (blog == null)
                return NotFound();

            await _blogRepository.DeleteAsync(blog);
            await _blogRepository.SaveAsync();

            return RedirectToAction("Manage");
        }

    }
}
