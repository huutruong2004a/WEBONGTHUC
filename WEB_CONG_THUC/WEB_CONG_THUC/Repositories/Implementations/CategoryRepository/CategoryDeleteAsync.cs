using Microsoft.EntityFrameworkCore;
using WEB_CONG_THUC.Data;
using WEB_CONG_THUC.Repositories.Interfaces.ICategoryRepository;

namespace WEB_CONG_THUC.Repositories.Implementations.CategoryRepository
{
    public class CategoryDeleteAsync : ICategoryDeleteAsync
    {
        private readonly ApplicationDbContext _context;
        public CategoryDeleteAsync(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DeleteAsync(string slug)
        {
            var product = await _context.Categories.FirstOrDefaultAsync(x => x.Slug == slug);
            if (product == null)
                throw new KeyNotFoundException($"Danh mục {slug} không tồn tại.");

            _context.Categories.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
