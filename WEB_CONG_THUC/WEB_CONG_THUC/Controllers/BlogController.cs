using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> Index()
        {
            var blogs = await _blogRepository.GetAllAsync();
            return View(blogs);
        }

        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            var blog = await _blogRepository.GetBySlugAsync(slug);
            if (blog == null)
                return NotFound();

            return View(blog);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog)
        {
            if (ModelState.IsValid)
            {
                if (blog.ImageFile != null)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(blog.ImageFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await blog.ImageFile.CopyToAsync(stream);
                    }

                    blog.ImageUrl = "/uploads/" + fileName;
                }

                blog.Slug = SlugHelper.GenerateSlug(blog.Title);
                blog.CreatedAt = DateTime.Now;

                await _blogRepository.AddAsync(blog);
                await _blogRepository.SaveAsync();

                return RedirectToAction("Index");
            }
            return View(blog);
        }

    }
}
