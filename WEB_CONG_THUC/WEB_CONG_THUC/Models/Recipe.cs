using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WEB_CONG_THUC.Models
{
    //công thức nấu ăn
    public class Recipe
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên công thức")]
        [StringLength(100, ErrorMessage = "Tên công thức không được vượt quá 100 ký tự")]
        [Display(Name = "Tên công thức")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        [NotMapped]
        [Display(Name = "Tải lên hình ảnh")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Thời gian chuẩn bị (phút)")]
        [Range(0, int.MaxValue, ErrorMessage = "Thời gian chuẩn bị phải là số dương")]
        public int PrepTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian nấu")]
        [Display(Name = "Thời gian nấu (phút)")]
        [Range(1, int.MaxValue, ErrorMessage = "Thời gian nấu phải lớn hơn 0")]
        public int CookTime { get; set; }

        [Display(Name = "Khẩu phần (người)")]
        [Range(1, int.MaxValue, ErrorMessage = "Số người dùng phải lớn hơn 0")]
        public int Servings { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nguyên liệu")]
        [Display(Name = "Nguyên liệu (mỗi dòng một nguyên liệu)")]
        public string Ingredients { get; set; } = string.Empty;

        // Navigation property for reviews
        public virtual ICollection<RecipeReview> Reviews { get; set; } = new List<RecipeReview>();

        [Required(ErrorMessage = "Vui lòng nhập hướng dẫn")]
        [Display(Name = "Các bước thực hiện (mỗi dòng một bước)")]
        public string Instructions { get; set; } = string.Empty;

        [Display(Name = "Video URL (YouTube, Vimeo...)")]
        [Url(ErrorMessage = "URL video không hợp lệ")]
        public string? VideoUrl { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        public virtual ICollection<RecipeFavorite> Favorites { get; set; } = new List<RecipeFavorite>();

        // public ICollection<RecipeReview> Reviews { get; set; } = new List<RecipeReview>(); // Dòng này đã bị comment hoặc xóa ở lần trước

        [Display(Name = "Phổ biến")]
        public bool IsPopular { get; set; } = false;
    }
}
