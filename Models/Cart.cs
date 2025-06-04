using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web_Ban_Dong_Ho.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public ICollection<CartProduct> Products { get; set; }

        public decimal CartTotal { get; set; }

        public decimal TotalAfterDiscount { get; set; } = 0;

        [Required]
        public int OrderById { get; set; }
        public User OrderBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
