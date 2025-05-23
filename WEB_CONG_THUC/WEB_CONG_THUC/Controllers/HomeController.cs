using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WEB_CONG_THUC.Models;
using WEB_CONG_THUC.Repositories;

namespace WEB_CONG_THUC.Controllers;

public class HomeController : Controller
{
    private readonly IBlogRepository _blogRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IBlogRepository blogRepository, IRecipeRepository recipeRepository, ILogger<HomeController> logger)
    {
        _logger = logger;
        _blogRepository = blogRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<IActionResult> Index()
    {
        var topBlogs = _blogRepository.GetTopViewedBlogs(3);
        var latestRecipes = await _recipeRepository.GetLatestRecipesAsync(6);

        var viewModel = new HomeViewModel
        {
            TopBlogs = topBlogs.ToList(),
            LatestRecipes = latestRecipes.ToList()
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
