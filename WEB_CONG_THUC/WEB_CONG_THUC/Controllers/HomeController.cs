using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WEB_CONG_THUC.Models;
using WEB_CONG_THUC.Repositories;

namespace WEB_CONG_THUC.Controllers;

public class HomeController : Controller
{
    private readonly IBlogRepository _blogRepository;

    private readonly ILogger<HomeController> _logger;

    public HomeController(IBlogRepository blogRepository,ILogger<HomeController> logger)
    {
        _logger = logger;
        _blogRepository = blogRepository;
    }

    public IActionResult Index()
    {
        var topBlogs = _blogRepository.GetTopViewedBlogs(3); // Lấy 3 blog nhiều lượt xem nhất
        return View(topBlogs);
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
