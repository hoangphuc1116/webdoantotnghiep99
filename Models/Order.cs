using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web_Ban_Dong_Ho.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public ICollection<OrderProduct> Products { get; set; }

        public string PaymentIntentJson { get; set; } // lưu object JSON như paymentIntent

        [Required]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        [Required]
        public int OrderById { get; set; }
        public User OrderBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
