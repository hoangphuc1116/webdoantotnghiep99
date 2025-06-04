using System;
using System.ComponentModel.DataAnnotations;

namespace Web_Ban_Dong_Ho.Models
{
    public class ProductRating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Star { get; set; }

        public string Comment { get; set; }

        [Required]
        public int UserId { get; set; }
        public User PostedBy { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
