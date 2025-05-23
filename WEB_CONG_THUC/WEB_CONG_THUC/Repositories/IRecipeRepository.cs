using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Repositories
{
    public interface IRecipeRepository
    {
        Task<IEnumerable<Recipe>> GetAllAsync();
        Task<IEnumerable<Recipe>> GetLatestRecipesAsync(int count);
        Task<Recipe> GetByIdAsync(int id);
        // Thêm các phương thức khác nếu cần, ví dụ: lấy công thức theo danh mục, tìm kiếm,...
    }
}