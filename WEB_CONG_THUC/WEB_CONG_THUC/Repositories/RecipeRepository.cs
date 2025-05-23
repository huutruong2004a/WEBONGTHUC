using Microsoft.EntityFrameworkCore;
using WEB_CONG_THUC.Data;
using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly ApplicationDbContext _context;

        public RecipeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Recipe>> GetAllAsync()
        {
            return await _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetLatestRecipesAsync(int count)
        {
            return await _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Recipe?> GetByIdAsync(int id)
        {
            return await _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.Reviews)
                    .ThenInclude(review => review.User)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}