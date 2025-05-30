using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_CONG_THUC.Models
{
    public class RecipeFavorite
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        public int RecipeId { get; set; }
        [ForeignKey("RecipeId")]
        public virtual Recipe? Recipe { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
