using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Models
{
    public class Video
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề video")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả video")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập URL video")]
        [Display(Name = "URL Video")]
        public string VideoUrl { get; set; } = string.Empty;

        [Display(Name = "Hình thu nhỏ")]
        public string? ThumbnailUrl { get; set; }

        [NotMapped]
        public IFormFile? ThumbnailFile { get; set; }

        public int? RecipeId { get; set; }
        [ForeignKey("RecipeId")]
        public virtual Recipe? Recipe { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ViewCount { get; set; } = 0;

        public virtual ICollection<VideoFavorite> Favorites { get; set; } = new List<VideoFavorite>();
    }
}