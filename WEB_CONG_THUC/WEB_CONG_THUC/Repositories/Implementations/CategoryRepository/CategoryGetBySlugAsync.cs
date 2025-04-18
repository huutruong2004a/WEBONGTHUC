using Microsoft.EntityFrameworkCore;
using WEB_CONG_THUC.Data;
using WEB_CONG_THUC.Models;
using WEB_CONG_THUC.Repositories.Interfaces.ICategoryRepository;

namespace WEB_CONG_THUC.Repositories.Implementations.CategoryRepository
{
    public class CategoryGetBySlugAsync : ICategoryGetBySlugAsync
    {
        private readonly ApplicationDbContext _context;

        public CategoryGetBySlugAsync(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _context.Categories
                .Include(d => d.Cook)
                .FirstOrDefaultAsync(d => d.Slug == slug);
        }
    }
}
 