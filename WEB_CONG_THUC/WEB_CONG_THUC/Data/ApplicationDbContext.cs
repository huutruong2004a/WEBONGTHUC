using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Cooking> Cookings { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Recipe> Recipes { get; set; }

    public DbSet<Video> Videos { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình khóa ngoại để tránh lỗi cascade
        modelBuilder.Entity<Recipe>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction); // Thay đổi từ Cascade thành NoAction

        modelBuilder.Entity<RecipeReview>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction); // Thay đổi từ Cascade thành NoAction

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Bữa sáng" },
            new Category { Id = 2, Name = "Bữa trưa" },
            new Category { Id = 3, Name = "Bữa tối" },
            new Category { Id = 4, Name = "Tráng miệng" },
            new Category { Id = 5, Name = "Món khai vị" },
            new Category { Id = 6, Name = "Món chay" },
            new Category { Id = 7, Name = "Đồ uống" },
            new Category { Id = 8, Name = "Bánh ngọt" },
            new Category { Id = 9, Name = "Món ăn nhanh" },
            new Category { Id = 10, Name = "Món ăn truyền thống" }
        );
    }
}
