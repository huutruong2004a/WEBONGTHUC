using System.Runtime.CompilerServices;

namespace WEB_CONG_THUC.Repositories.Interfaces.ICategoryRepository
{
    public interface ICategoryDeleteAsync
    {
        Task DeleteAsync(string slug);

    }
}
