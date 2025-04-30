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
                blog.Slug = SlugHelper.GenerateSlug(blog.Title);
                blog.CreatedAt = DateTime.Now;

                await _blogRepository.AddAsync(blog);
                await _blogRepository.SaveAsync();

                return RedirectToAction("Index");
            }
           
            return View(blog);
        }


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

    }
}
