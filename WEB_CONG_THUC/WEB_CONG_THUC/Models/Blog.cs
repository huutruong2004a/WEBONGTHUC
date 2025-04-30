using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_CONG_THUC.Models
{
    public class Blog
    {
        public int Id { get; set; }

        
        public required string Title { get; set; }

        
        public required string Content { get; set; } // HTML nội dung có ảnh

        public string Slug { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
    }

}
