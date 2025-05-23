using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.Models
{
    public class Video
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề video")]
        [StringLength(100, ErrorMessage = "Tiêu đề không được vượt quá 100 ký tự")]
        public string? Title { get; set; }

        [AllowNull]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập URL video")]
        public string? VideoUrl { get; set; }

        [AllowNull]
        public string? ThumbnailUrl { get; set; }

        public int? CategoryId { get; set; }

        public string? AuthorName { get; set; }

        [AllowNull]
        public string? AuthorEmail { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
    }
}