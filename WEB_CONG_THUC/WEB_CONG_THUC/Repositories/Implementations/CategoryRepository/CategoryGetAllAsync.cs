using Microsoft.EntityFrameworkCore;
using WEB_CONG_THUC.Data;
using WEB_CONG_THUC.Models;
using WEB_CONG_THUC.Repositories.Interfaces.ICategoryRepository;

namespace WEB_CONG_THUC.Repositories.Implementations.CategoryRepository
{
    public class CategoryGetAllAsync : ICategoryGetAllAsync
    {
        private ApplicationDbContext _context;
        public CategoryGetAllAsync(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                                .Include(p => p.Cook) 
                                .ToListAsync();
        }
    }
}
