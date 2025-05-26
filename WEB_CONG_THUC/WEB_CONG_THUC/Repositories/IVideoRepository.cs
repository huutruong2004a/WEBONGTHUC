

using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Repositories
{
    public interface IVideoRepository
    {
        Task<IEnumerable<Video>> GetAllAsync();
        Task<IEnumerable<Video>> GetVideosByRecipeAsync(int recipeId);
        Task<Video?> GetByIdAsync(int id);
        Task<IEnumerable<Video>> GetByRecipeIdAsync(int recipeId);
        Task<IEnumerable<Video>> GetFavoritesByUserIdAsync(string userId);
        Task<bool> AddAsync(Video video);
        Task<bool> UpdateAsync(Video video);
        Task<bool> ToggleFavoriteAsync(int videoId, string userId);
    }
}
