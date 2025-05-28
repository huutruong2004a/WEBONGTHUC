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
    public DbSet<VideoFavorite> VideoFavorites { get; set; }
    public DbSet<VideoComment> VideoComments { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraint cho VideoFavorite
        modelBuilder.Entity<VideoFavorite>()
            .HasIndex(vf => new { vf.UserId, vf.VideoId })
            .IsUnique();

        // Cấu hình khóa ngoại tránh lỗi cascade cycles
        modelBuilder.Entity<Video>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.NoAction); // hoặc DeleteBehavior.Restrict

        modelBuilder.Entity<VideoFavorite>()
            .HasOne(vf => vf.User)
            .WithMany()
            .HasForeignKey(vf => vf.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<VideoFavorite>()
            .HasOne(vf => vf.Video)
            .WithMany(v => v.Favorites)
            .HasForeignKey(vf => vf.VideoId)
            .OnDelete(DeleteBehavior.Cascade); // giữ lại cascade với Video nếu bạn muốn

        modelBuilder.Entity<Recipe>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<RecipeReview>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Seed dữ liệu Categories
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
