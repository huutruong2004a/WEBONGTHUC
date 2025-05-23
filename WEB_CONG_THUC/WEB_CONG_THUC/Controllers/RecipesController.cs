using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WEB_CONG_THUC.Data;
using WEB_CONG_THUC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CookShare.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RecipesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Recipes
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["Categories"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name", categoryId);

            var recipesQuery = _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .AsQueryable(); // Chuyển sang IQueryable để xây dựng truy vấn động

            if (!String.IsNullOrEmpty(searchString))
            {
                recipesQuery = recipesQuery.Where(r => r.Title.Contains(searchString) ||
                                                   (r.Description != null && r.Description.Contains(searchString)) ||
                                                   (r.User != null && r.User.UserName != null && r.User.UserName.Contains(searchString)));
            }

            if (categoryId.HasValue)
            {
                recipesQuery = recipesQuery.Where(r => r.CategoryId == categoryId.Value);
            }

            var recipes = await recipesQuery.ToListAsync();

            return View(recipes);
        }

        // GET: Recipes/MyRecipes
        [Authorize]
        public async Task<IActionResult> MyRecipes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var recipes = await _context.Recipes
                .Include(r => r.Category)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(recipes);
        }

        // GET: Recipes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.Reviews)
                    .ThenInclude(review => review.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // GET: Recipes/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            var viewModel = new RecipeCreateViewModel(); // Khởi tạo ViewModel nếu cần giá trị mặc định
            return View(viewModel);
        }

        // POST: Recipes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(RecipeCreateViewModel viewModel) // Sử dụng ViewModel ở đây
        {
            if (ModelState.IsValid) // Kiểm tra ModelState của ViewModel
            {
                var recipe = new Recipe
                {
                    // Map các thuộc tính từ viewModel sang recipe
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    PrepTime = viewModel.PrepTime,
                    CookTime = viewModel.CookTime,
                    Servings = viewModel.Servings,
                    CategoryId = viewModel.CategoryId,
                    Ingredients = viewModel.Ingredients,
                    Instructions = viewModel.Instructions,
                    VideoUrl = viewModel.VideoUrl,

                    // Gán các giá trị phía server
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Xử lý upload hình ảnh
                if (viewModel.ImageFile != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(viewModel.ImageFile.FileName);
                    string extension = Path.GetExtension(viewModel.ImageFile.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    string path = Path.Combine(wwwRootPath + "/images/recipes/", fileName);

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await viewModel.ImageFile.CopyToAsync(fileStream);
                    }
                    recipe.ImageUrl = "/images/recipes/" + fileName;
                }

                _context.Add(recipe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyRecipes));
            }

            // Nếu ModelState không hợp lệ, quay lại view Create với ViewModel và các lỗi
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", viewModel.CategoryId);
            return View(viewModel);
        }

        // GET: Recipes/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            // Check if the user is the owner of the recipe
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (recipe != null && recipe.UserId != userId)
            {
                return Forbid();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", recipe.CategoryId);
            return View(recipe);
        }

        // POST: Recipes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Recipe recipe)
        {
            if (id != recipe.Id)
            {
                return NotFound();
            }

            var existingRecipe = await _context.Recipes.FindAsync(id);
            if (existingRecipe == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingRecipe.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (recipe.ImageFile != null)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingRecipe.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingRecipe.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save new image
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(recipe.ImageFile.FileName);
                        string extension = Path.GetExtension(recipe.ImageFile.FileName);
                        fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "/images/recipes/", fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await recipe.ImageFile.CopyToAsync(fileStream);
                        }

                        recipe.ImageUrl = "/images/recipes/" + fileName;
                    }
                    else
                    {
                        // Keep the existing image if no new image is uploaded
                        recipe.ImageUrl = existingRecipe.ImageUrl;
                    }

                    // Update other properties
                    existingRecipe.Title = recipe.Title;
                    existingRecipe.Description = recipe.Description;
                    existingRecipe.ImageUrl = recipe.ImageUrl;
                    existingRecipe.PrepTime = recipe.PrepTime;
                    existingRecipe.CookTime = recipe.CookTime;
                    existingRecipe.Servings = recipe.Servings;
                    existingRecipe.CategoryId = recipe.CategoryId;
                    existingRecipe.Ingredients = recipe.Ingredients;
                    existingRecipe.Instructions = recipe.Instructions;
                    existingRecipe.VideoUrl = recipe.VideoUrl;
                    existingRecipe.UpdatedAt = DateTime.Now;

                    _context.Update(existingRecipe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeExists(recipe.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(MyRecipes));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", recipe.CategoryId);
            return View(recipe);
        }

        // GET: Recipes/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Check if the user is the owner of the recipe
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (recipe.UserId != userId)
            {
                return Forbid();
            }

            return View(recipe);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (recipe.UserId != userId)
            {
                return Forbid();
            }

            // Delete image file if exists
            if (!string.IsNullOrEmpty(recipe.ImageUrl))
            {
                var imagePath = Path.Combine(_hostEnvironment.WebRootPath, recipe.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Recipes.Remove(recipe!);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyRecipes));
        }

        private bool RecipeExists(int id)
        {
            return _context.Recipes.Any(e => e.Id == id);
        }
    }
}