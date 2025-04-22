using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_CONG_THUC.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string? ImageUrl { get; set; } // Ảnh (nếu có)

        public string? VideoUrl { get; set; } // Video (nếu có)

        public string Slug { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [NotMapped]
        public IFormFile? ImageFile { get; set; } // File ảnh từ form
    }
}
