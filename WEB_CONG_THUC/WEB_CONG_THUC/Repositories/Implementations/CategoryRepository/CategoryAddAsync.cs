using Microsoft.EntityFrameworkCore;
using WEB_CONG_THUC.Data;
using WEB_CONG_THUC.Models;
using WEB_CONG_THUC.Repositories.Interfaces.ICategoryRepository;

namespace WEB_CONG_THUC.Repositories.Implementations.CategoryRepository
{
    public class CategoryAddAsync: ICategoryAddAsync
    {
        private ApplicationDbContext _context;
        public CategoryAddAsync(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }
    }
}
