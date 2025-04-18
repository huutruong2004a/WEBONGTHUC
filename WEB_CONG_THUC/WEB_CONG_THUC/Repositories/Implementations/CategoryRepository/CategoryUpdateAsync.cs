using WEB_CONG_THUC.Data;
using WEB_CONG_THUC.Models;
using WEB_CONG_THUC.Repositories.Interfaces.ICategoryRepository;

namespace WEB_CONG_THUC.Repositories.Implementations.CategoryRepository
{
    public class CategoryUpdateAsync : ICategoryUpdateAsync
    {
        private readonly ApplicationDbContext _context;
        public CategoryUpdateAsync(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UpdateAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}
