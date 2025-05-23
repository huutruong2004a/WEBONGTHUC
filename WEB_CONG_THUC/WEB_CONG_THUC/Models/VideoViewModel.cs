using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using WEB_CONG_THUC.Models;

namespace WEB_CONG_THUC.ViewModels
{
    public class VideoIndexViewModel
    {
        public List<Video> Videos { get; set; }
        public List<Category> Categories { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string SearchString { get; set; }
    }

    public class VideoCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề video")]
        [StringLength(100, ErrorMessage = "Tiêu đề không được vượt quá 100 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập URL video")]
        [Display(Name = "URL Video (YouTube, Vimeo, v.v.)")]
        public string VideoUrl { get; set; }

        [Display(Name = "Hình thu nhỏ")]
        public IFormFile ThumbnailFile { get; set; }

        [Display(Name = "Danh mục")]
        public int? CategoryId { get; set; }

        [Display(Name = "Tên người đăng")]
        public string AuthorName { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string AuthorEmail { get; set; }
    }

    public class VideoDetailsViewModel
    {
        public Video Video { get; set; }
        public List<Video> RelatedVideos { get; set; }
    }

    public class VideoAdminViewModel
    {
        public List<Video> PendingVideos { get; set; }
        public List<Video> ApprovedVideos { get; set; }
    }
}