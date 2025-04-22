using System.Collections.Generic;
using System.Threading.Tasks;
using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Repositories
{
    public interface IBlogRepository
    {
        Task<IEnumerable<Blog>> GetAllAsync();
        Task<Blog?> GetByIdAsync(int id);
        Task<Blog?> GetBySlugAsync(string slug);
        Task AddAsync(Blog blog);
        void Update(Blog blog);
        void Delete(Blog blog);
        Task SaveAsync();
        IEnumerable<Blog> GetLatestBlogs(int count);
    }
}
