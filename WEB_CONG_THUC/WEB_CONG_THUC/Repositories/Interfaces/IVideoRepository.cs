using System.Collections.Generic;
using System.Threading.Tasks;
using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Repositories
{
    public interface IVideoRepository
    {
        Task<IEnumerable<Video>> GetApprovedVideosAsync();
        Task<IEnumerable<Video>> GetVideosByCategoryAsync(int categoryId);
        Task<IEnumerable<Video>> GetLatestVideosAsync(int count);
        Task<IEnumerable<Video>> GetPendingVideosAsync();
        Task<IEnumerable<Video>> SearchVideosAsync(string searchTerm, int? categoryId = null);
    }
}
