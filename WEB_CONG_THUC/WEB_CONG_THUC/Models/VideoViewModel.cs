using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using WEB_CONG_THUC.Models;
using System.Diagnostics.CodeAnalysis;

namespace WEB_CONG_THUC.ViewModels
{
    public class VideoIndexViewModel
    {
        public List<Video> Videos { get; set; } = new List<Video>();
        public List<Category> Categories { get; set; } = new List<Category>();
        [AllowNull]
        public string? SearchString { get; set; }
        public int? SelectedCategoryId { get; set; }
    }

    public class VideoCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề video")]
        [StringLength(100, ErrorMessage = "Tiêu đề không được vượt quá 100 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string? Title { get; set; }

        [AllowNull]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập URL video")]
        [Display(Name = "URL Video (YouTube, Vimeo, v.v.)")]
        public string? VideoUrl { get; set; }
        [AllowNull]
        public IFormFile? ThumbnailFile { get; set; }

        [Display(Name = "Danh mục")]
        public int? CategoryId { get; set; }

        [Display(Name = "Tên người đăng")]
        public string? AuthorName { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        [AllowNull]
        public string? AuthorEmail { get; set; }
    }

    public class VideoDetailsViewModel
    {
        public Video? Video { get; set; }
        public List<Video> RelatedVideos { get; set; } = new List<Video>();
    }

    public class VideoAdminViewModel
    {
        public List<Video> PendingVideos { get; set; } = new List<Video>();
        public List<Video> ApprovedVideos { get; set; } = new List<Video>();
    }
}