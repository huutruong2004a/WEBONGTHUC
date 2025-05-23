using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WEB_CONG_THUC.Data;
using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly ApplicationDbContext _context;

        public VideoRepository(ApplicationDbContext context)
        {
            _context = context; // No need for base(context)
        }

        public async Task<IEnumerable<Video>> GetApprovedVideosAsync()
        {
            return await _context.Videos
                .Include(v => v.Category)
                .Where(v => v.IsApproved)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Video>> GetVideosByCategoryAsync(int categoryId)
        {
            return await _context.Videos
                .Include(v => v.Category)
                .Where(v => v.IsApproved && v.CategoryId == categoryId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Video>> GetLatestVideosAsync(int count)
        {
            return await _context.Videos
                .Include(v => v.Category)
                .Where(v => v.IsApproved)
                .OrderByDescending(v => v.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Video>> GetPendingVideosAsync()
        {
            return await _context.Videos
                .Include(v => v.Category)
                .Where(v => !v.IsApproved)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Video>> SearchVideosAsync(string searchTerm, int? categoryId = null)
        {
            var query = _context.Videos
                .Include(v => v.Category)
                .Where(v => v.IsApproved);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(v =>
                    v.Title.Contains(searchTerm) ||
                    v.Description.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(v => v.CategoryId == categoryId.Value);
            }

            return await query
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }
    }
}
