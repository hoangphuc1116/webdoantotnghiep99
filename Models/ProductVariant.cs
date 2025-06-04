using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Web_Ban_Dong_Ho.Models
{
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        public int CurrentId { get; set; }
        public Current Current { get; set; }

        public int ColorId { get; set; }
        public Color Color { get; set; }

        public int Quantity { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
