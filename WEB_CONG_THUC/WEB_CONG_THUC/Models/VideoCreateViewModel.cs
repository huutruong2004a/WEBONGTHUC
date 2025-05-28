using System.ComponentModel.DataAnnotations;

namespace WEB_CONG_THUC.Models
{
    public class VideoCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề video")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Loại upload")]
        public VideoUploadType UploadType { get; set; }

        [Display(Name = "URL Video")]
        public string? VideoUrl { get; set; }

        [Display(Name = "File Video")]
        public IFormFile? VideoFile { get; set; }

        [Display(Name = "Hình thu nhỏ")]
        public IFormFile? ThumbnailFile { get; set; }
    }
}
