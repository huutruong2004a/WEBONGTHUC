using Microsoft.EntityFrameworkCore;
using WEB_CONG_THUC.Data;
using WEB_CONG_THUC.Data.Migrations;
using WEB_CONG_THUC.Models;
using Video = WEB_CONG_THUC.Models.Video;


namespace WEB_CONG_THUC.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly ApplicationDbContext _context;

        public VideoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Video>> GetAllAsync()
        {
            return await _context.Videos
                .Include(v => v.Recipe)
                .Include(v => v.User)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<Video?> GetByIdAsync(int id)
        {
            return await _context.Videos
                .Include(v => v.Recipe)
                .Include(v => v.User)
                .Include(v => v.Favorites)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Video>> GetByRecipeIdAsync(int recipeId)
        {
            return await _context.Videos
                .Where(v => v.RecipeId == recipeId)
                .Include(v => v.User)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Video>> GetFavoritesByUserIdAsync(string userId)
        {
            return await _context.VideoFavorites
            .Include(vf => vf.Video)
                .ThenInclude(v => v!.Recipe)
            .Include(vf => vf.Video)
                .ThenInclude(v => v!.User)
            .Where(vf => vf.UserId == userId)
            .Select(vf => vf.Video!)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
        }
        public async Task<IEnumerable<Video>> GetVideosByRecipeAsync(int recipeId)
        {
            return await _context.Videos
             .Include(v => v.Recipe)
             .Include(v => v.User)
             .Include(v => v.Favorites)
             .Where(v => v.RecipeId == recipeId)
             .OrderByDescending(v => v.CreatedAt)
             .ToListAsync();

        }
        public async Task<bool> UpdateAsync(Video video)
        {
            _context.Videos.Update(video);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> AddAsync(Video video)
        {
            _context.Videos.Add(video);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleFavoriteAsync(int videoId, string userId)
        {
            var favorite = await _context.VideoFavorites
                .FirstOrDefaultAsync(vf => vf.VideoId == videoId && vf.UserId == userId);

            if (favorite != null)
            {
                _context.VideoFavorites.Remove(favorite);
            }
            else
            {
                _context.VideoFavorites.Add(new VideoFavorite
                {
                    VideoId = videoId,
                    UserId = userId
                });
            }

            return await _context.SaveChangesAsync() > 0;

        }

    }
}
