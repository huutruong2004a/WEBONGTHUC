using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEB_CONG_THUC.Data;
using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly ApplicationDbContext _context;

        public BlogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Blog>> GetAllAsync()
        {
            return await _context.Blogs.OrderByDescending(b => b.CreatedAt).ToListAsync();
        }
        public async Task<List<Blog>> GetApprovedAsync()
        {
            return await _context.Blogs
                .Where(b => b.Status == BlogStatus.Approved)
                .ToListAsync();
        }
        public async Task<Blog?> GetByIdAsync(int id)
        {
            return await _context.Blogs.FindAsync(id);
        }

        public async Task<Blog?> GetBySlugAsync(string slug)
        {
            return await _context.Blogs.FirstOrDefaultAsync(b => b.Slug == slug);
        }

        public async Task AddAsync(Blog blog)
        {
            await _context.Blogs.AddAsync(blog);
        }

        public Task UpdateAsync(Blog blog)
        {
            _context.Blogs.Update(blog);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Blog blog)
        {
            _context.Blogs.Remove(blog);
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IEnumerable<Blog> GetLatestBlogs(int count)
        {
            return _context.Blogs.OrderByDescending(b => b.CreatedAt).Take(count).ToList();
        }
        public IEnumerable<Blog> GetTopViewedBlogs(int count)
        {
            return _context.Blogs
                           .OrderByDescending(b => b.ViewCount)
                           .Take(count)
                           .ToList();
        }
    }
}
