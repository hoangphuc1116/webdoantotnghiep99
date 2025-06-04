using System.ComponentModel.DataAnnotations;

namespace Web_Ban_Dong_Ho.Models
{
    public class Color
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)] // Giới hạn tùy ý
        public string Title { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
