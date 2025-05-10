using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WEB_CONG_THUC.Models
{
    public class RecipeReview
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }
    }
}
