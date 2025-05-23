using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Models
{
    public class HomeViewModel
    {
        public List<Blog> TopBlogs { get; set; } = new List<Blog>();
        public List<Recipe> LatestRecipes { get; set; } = new List<Recipe>();
    }
}