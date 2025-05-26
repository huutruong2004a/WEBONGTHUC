using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_CONG_THUC.Models
{
    public class VideoFavorite
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int VideoId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [ForeignKey("VideoId")]
        public virtual Video? Video { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
