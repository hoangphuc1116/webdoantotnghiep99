using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web_Ban_Dong_Ho.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Slug { get; set; }

        public string Description { get; set; }

        public decimal? PriceOld { get; set; }

        [Required]
        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public int Sold { get; set; } = 0;

        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        public int? BrandId { get; set; }
        public Brand Brand { get; set; }

        public ICollection<ProductImage> Image { get; set; }

        public ICollection<ProductVariant> Variants { get; set; }

        public ICollection<ProductRating> Ratings { get; set; }

        public double TotalRating { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
