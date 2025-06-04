using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web_Ban_Dong_Ho.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Firstname { get; set; }

        [Required]
        public string Lastname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Mobile { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; } = "user";

        public bool IsBlocked { get; set; } = false;

        // Cart có thể dùng một bảng riêng nếu cần lưu chi tiết từng sản phẩm
        public string CartJson { get; set; } = "[]"; // Giải pháp đơn giản: lưu JSON dạng chuỗi

        public string Address { get; set; }

        // Hình ảnh có thể là danh sách URL
        public string ImagesJson { get; set; } = "[]"; // Lưu JSON danh sách URL ảnh

        // Wishlist có thể liên kết với Product thông qua khóa ngoại
        public ICollection<WishlistItem> Wishlist { get; set; }

        public string RefreshToken { get; set; }

        public DateTime? PasswordChangedAt { get; set; }

        public string PasswordResetToken { get; set; }

        public DateTime? PasswordResetExpires { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
