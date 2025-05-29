using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Repositories.Interfaces.ICategoryRepository
{
    public interface ICategoryUpdateAsync
    {
        Task UpdateAsync(Category category);

    }
}
