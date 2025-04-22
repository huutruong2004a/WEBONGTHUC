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

        public void Update(Blog blog)
        {
            _context.Blogs.Update(blog);
        }

        public void Delete(Blog blog)
        {
            _context.Blogs.Remove(blog);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IEnumerable<Blog> GetLatestBlogs(int count)
        {
            return _context.Blogs.OrderByDescending(b => b.CreatedAt).Take(count).ToList();
        }
    }
}
