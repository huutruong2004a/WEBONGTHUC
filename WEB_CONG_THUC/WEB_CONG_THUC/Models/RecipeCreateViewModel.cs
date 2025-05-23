using Microsoft.AspNetCore.Http; // Cần cho IFormFile
using System.ComponentModel.DataAnnotations;

namespace WEB_CONG_THUC.Models
{
    public class RecipeCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên công thức")]
        [StringLength(100, ErrorMessage = "Tên công thức không được vượt quá 100 ký tự")]
        [Display(Name = "Tên công thức")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Tải lên hình ảnh")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Thời gian chuẩn bị (phút)")]
        [Range(0, int.MaxValue, ErrorMessage = "Thời gian chuẩn bị phải là số dương")]
        public int PrepTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian nấu")]
        [Display(Name = "Thời gian nấu (phút)")]
        [Range(1, int.MaxValue, ErrorMessage = "Thời gian nấu phải lớn hơn 0")]
        public int CookTime { get; set; }

        [Display(Name = "Số người dùng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số người dùng phải lớn hơn 0")]
        public int Servings { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nguyên liệu")]
        [Display(Name = "Nguyên liệu (mỗi dòng một nguyên liệu)")]
        public string Ingredients { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập các bước thực hiện")]
        [Display(Name = "Các bước thực hiện (mỗi dòng một bước)")]
        public string Instructions { get; set; } = string.Empty;

        [Display(Name = "Video URL (YouTube, Vimeo...)")]
        [Url(ErrorMessage = "URL video không hợp lệ")]
        public string? VideoUrl { get; set; }
    }
}