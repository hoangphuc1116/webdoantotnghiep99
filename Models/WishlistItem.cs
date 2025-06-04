using System.ComponentModel.DataAnnotations;
using Web_Ban_Dong_Ho.Models;

public class WishlistItem
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
}
