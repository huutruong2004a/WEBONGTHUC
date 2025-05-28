using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Models
{
    public enum VideoStatus
    {
        [Display(Name = "Chờ duyệt")]
        Pending,
        [Display(Name = "Đã duyệt")]
        Approved,
        [Display(Name = "Từ chối")]
        Rejected
    }
    public class Video
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề video")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả video")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Loại upload")]
        public VideoUploadType UploadType { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập URL video")]
        [Display(Name = "URL Video")]
        public string VideoUrl { get; set; } = string.Empty;

        [NotMapped]
        [Display(Name = "File Video")]
        public IFormFile? VideoFile { get; set; }

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

        public virtual ICollection<VideoComment> Comments { get; set; } = new HashSet<VideoComment>();
        public string Slug { get; set; } = string.Empty;
        public VideoStatus Status { get; set; } = VideoStatus.Pending;
    }
    public enum VideoUploadType
    {
        [Display(Name = "URL")]
        Url,
        [Display(Name = "File")]
        File
    }
}