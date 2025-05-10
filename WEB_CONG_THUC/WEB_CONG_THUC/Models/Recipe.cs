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
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; }

        public string ImageUrl { get; set; }

        [NotMapped]
        [Display(Name = "Tải lên hình ảnh")]
        public IFormFile ImageFile { get; set; }


        public int PrepTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian nấu")]

        public int CookTime { get; set; }

        public int Servings { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nguyên liệu")]
        public string Ingredients { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập các bước thực hiện")]
        [Display(Name = "Các bước thực hiện")]
        public string Instructions { get; set; }

        [Display(Name = "Video URL")]
        public string VideoUrl { get; set; }

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        public ICollection<RecipeReview> Reviews { get; set; }
    }
}
