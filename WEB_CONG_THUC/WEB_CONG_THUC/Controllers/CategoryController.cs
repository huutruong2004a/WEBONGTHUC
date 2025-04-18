using Microsoft.AspNetCore.Mvc;
using WEB_CONG_THUC.Models;
using WEB_CONG_THUC.Repositories.Interfaces.ICategoryRepository;

namespace WEB_CONG_THUC.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryGetAllAsync _category;
        private readonly ICategoryAddAsync _categoryAdd;
        private readonly ICategoryGetBySlugAsync _GetSlug;
        private readonly ICategoryUpdateAsync _categoryUpdate;
        private readonly ICategoryDeleteAsync _categoryDelete;
        public CategoryController(ICategoryGetAllAsync categoryGetAllAsync, ICategoryAddAsync categoryAdd, ICategoryGetBySlugAsync getSlug
            , ICategoryUpdateAsync categoryUpdate, ICategoryDeleteAsync categoryDelete)
        {
            _category = categoryGetAllAsync;
            _categoryAdd = categoryAdd;
            _GetSlug = getSlug;
            _categoryUpdate = categoryUpdate;
            _categoryDelete = categoryDelete;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _category.GetAllAsync();
            return View(categories);
        }

        //add
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryAdd.AddAsync(category);
                return RedirectToAction("Index", "Category");
            }
            return View(category);
        }

        //edit
        public async Task<IActionResult> Update(string slug)
        {
            var category = await _GetSlug.GetBySlugAsync(slug);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string slug, [Bind("Id, Name")] Category category)
        {
            if (slug != category.Slug)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _categoryUpdate.UpdateAsync(category);
                return RedirectToAction("Index", "Category");
            }
            return View(category);
        }

        //delete

        public async Task<IActionResult> Delete(string slug)
        {
            var category = await _GetSlug.GetBySlugAsync(slug);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string slug)
        {
            var category = await _GetSlug.GetBySlugAsync(slug);
            if (category == null)
            {
                return NotFound();
            }
            await _categoryDelete.DeleteAsync(slug);
            return RedirectToAction("Index", "Category");
        }

        //detail
        public async Task<IActionResult> Display(string slug)
        {
            var category = await _GetSlug.GetBySlugAsync(slug);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
    }
}
