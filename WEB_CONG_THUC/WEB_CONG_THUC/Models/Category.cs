using System.ComponentModel.DataAnnotations;

namespace WEB_CONG_THUC.Models
{
    public class Category
    {

        public int Id { get; set; }

        [Required, StringLength(50)]
        public required string Name { get; set; }
        public string? Slug { get; set; }

        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    }
}
