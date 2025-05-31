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
using WEB_CONG_THUC.Repositories;

namespace CookShare.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IVideoRepository _videoRepository;
        private readonly IRecipeRepository _recipeRepository;
        public RecipesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, IVideoRepository videoRepository,
            IRecipeRepository recipeRepository)
        {
            _videoRepository = videoRepository;
            _recipeRepository = recipeRepository;
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
                string lowerSearchString = searchString.ToLower(); // Chuyển searchString về chữ thường một lần
                recipesQuery = recipesQuery.Where(r =>
                    (r.Title != null && r.Title.ToLower().Contains(lowerSearchString)) ||
                    (r.Description != null && r.Description.ToLower().Contains(lowerSearchString)) ||
                    (r.Ingredients != null && r.Ingredients.ToLower().Contains(lowerSearchString)) || // Thêm tìm kiếm trong Ingredients
                    (r.User != null && r.User.UserName != null && r.User.UserName.ToLower().Contains(lowerSearchString))
                );
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

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.IsRecipeFavorited = await _context.RecipeFavorites
                                                .AnyAsync(rf => rf.RecipeId == id && rf.UserId == userId);
            }
            else
            {
                ViewBag.IsRecipeFavorited = false;
            }

            return View(recipe);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int recipeId, int rating, string? comment, string? returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge(); // Hoặc Unauthorized() tùy theo logic của bạn
            }

            // Kiểm tra xem có rating hoặc comment không, nếu cả hai đều trống thì báo lỗi
            if (rating == 0 && string.IsNullOrWhiteSpace(comment))
            {
                TempData["ErrorMessage"] = "Vui lòng cung cấp đánh giá (sao) hoặc viết bình luận.";
                return Redirect(returnUrl ?? Url.Action("Details", "Recipes", new { id = recipeId })!);
            }

            var review = new RecipeReview
            {
                RecipeId = recipeId,
                UserId = userId,
                Rating = rating, // Rating có thể là 0 nếu người dùng chỉ comment
                Comment = comment ?? string.Empty, // Comment có thể trống nếu người dùng chỉ đánh giá sao
                CreatedAt = DateTime.Now
            };

            _context.RecipeReviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi đánh giá!";
            return Redirect(returnUrl ?? Url.Action("Details", "Recipes", new { id = recipeId })!);
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
            if (ModelState.IsValid)
            {
                var recipe = new Recipe
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    PrepTime = viewModel.PrepTime,
                    CookTime = viewModel.CookTime,
                    Servings = viewModel.Servings,
                    CategoryId = viewModel.CategoryId,
                    Ingredients = viewModel.Ingredients,
                    Instructions = viewModel.Instructions,
                    VideoUrl = viewModel.VideoUrl,
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

                // Thêm recipe vào database
                _context.Add(recipe);
                await _context.SaveChangesAsync();

                // Nếu có URL video, tạo bản ghi video mới
                if (!string.IsNullOrEmpty(viewModel.VideoUrl))
                {
                    var video = new Video
                    {
                        Title = $"Video hướng dẫn - {recipe.Title}",
                        Description = $"Video hướng dẫn cho công thức {recipe.Title}",
                        VideoUrl = viewModel.VideoUrl,
                        RecipeId = recipe.Id,
                        UserId = recipe.UserId,
                        CreatedAt = DateTime.Now
                    };

                    // Xử lý thumbnail từ YouTube nếu là URL YouTube
                    if (viewModel.VideoUrl.Contains("youtube.com/watch?v=") || viewModel.VideoUrl.Contains("youtu.be/"))
                    {
                        string videoId = "";
                        if (viewModel.VideoUrl.Contains("youtube.com/watch?v="))
                        {
                            videoId = viewModel.VideoUrl.Split("v=")[1].Split('&')[0];
                        }
                        else if (viewModel.VideoUrl.Contains("youtu.be/"))
                        {
                            videoId = viewModel.VideoUrl.Split('/').Last().Split('?')[0];
                        }

                        if (!string.IsNullOrEmpty(videoId))
                        {
                            // Sử dụng thumbnail của YouTube
                            video.ThumbnailUrl = $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
                        }
                    }

                    // Thêm video vào database
                    await _videoRepository.AddAsync(video);
                }

                return RedirectToAction(nameof(MyRecipes));
            }

            // Nếu ModelState không hợp lệ
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
            if (recipe.UserId != userId) // recipe is already checked for null
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

        [Authorize]
        public async Task<IActionResult> MyFavoriteRecipes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var favoriteRecipes = await _context.RecipeFavorites
                                        .Where(rf => rf.UserId == userId)
                                        .Include(rf => rf.Recipe) // Tải thông tin Recipe
                                            .ThenInclude(r => r!.User) // Tải thông tin User của Recipe
                                        .Include(rf => rf.Recipe) // Tải thông tin Recipe
                                            .ThenInclude(r => r!.Category) // Tải thông tin Category của Recipe
                                        .Select(rf => rf.Recipe)
                                        .ToListAsync();
            return View(favoriteRecipes);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken] // Cân nhắc bỏ nếu gọi từ AJAX thuần túy và xử lý anti-forgery token riêng
        public async Task<IActionResult> ToggleFavoriteRecipe(int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            var existingFavorite = await _context.RecipeFavorites
                                        .FirstOrDefaultAsync(rf => rf.RecipeId == recipeId && rf.UserId == userId);

            bool isFavorited;
            if (existingFavorite != null)
            {
                _context.RecipeFavorites.Remove(existingFavorite);
                isFavorited = false;
            }
            else
            {
                _context.RecipeFavorites.Add(new RecipeFavorite { RecipeId = recipeId, UserId = userId });
                isFavorited = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isFavorited });
        }

        public async Task<IActionResult> Videos(int id)
        {
            var videos = await _videoRepository.GetVideosByRecipeAsync(id);
            var recipe = await _recipeRepository.GetByIdAsync(id);

            if (recipe == null)
            {
                return NotFound();
            }

            ViewBag.Recipe = recipe;
            return View(videos);
        }
    }
}