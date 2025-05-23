using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Blog> TopBlogs { get; set; }
        public IEnumerable<Recipe> LatestRecipes { get; set; }
    }
}