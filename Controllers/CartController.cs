using Ecommerce_WatchShop.Helper;
using Ecommerce_WatchShop.Models;
using Ecommerce_WatchShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Web_Ban_Dong_Ho.Helper;
using Web_Ban_Dong_Ho.Models;

public class CartController : Controller
{
    private readonly DongHoContext _context;

    public CartController(DongHoContext context)
    {
        _context = context;
    }

    public List<CartRequest> Carts => CartHelper.GetCart(HttpContext.Session);

    public IActionResult Cart(int page = 1, int pageSize = 5)
    {
        if (!User.Identity!.IsAuthenticated)
        {
            TempData["error"] = "Không có tài khoản của khách hàng";
            return View(Carts);
        }

        var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CustomerId");
        int? customerId = customerIdClaim != null ? int.Parse(customerIdClaim.Value) : (int?)null;

        if (customerId == null)
        {
            TempData["error"] = "Không tìm thấy tài khoản khách hàng";
            return View(Carts);
        }

        var cartItems = CartHelper.GetCart(HttpContext.Session);

        if (cartItems == null || !cartItems.Any())
        {

            return View("EmptyCart");
        }

        var totalItems = cartItems.Count;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var paginatedItems = cartItems
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(paginatedItems);
    }

    public async Task<IActionResult> AddToCart(string slug, int quantity)
    {
        Console.WriteLine($"Slug: {slug}, Quantity: {quantity}");
        var cart = Carts;
        var item = cart.FirstOrDefault(p => p.Slug == slug);
        if (item is null)
        {
            var products = await _context.Products.FirstOrDefaultAsync(p => p.Slug == slug);
            if (products == null)
            {
                return Json(new { success = false, message = $"Không tìm thấy sản phẩm có mã {slug}." });
            }
            item = new CartRequest
            {
                ProductId = products.ProductId,
                Slug = products.Slug!,
                ProductName = products.ProductName!,
                Image = products.Image,
                Price = products.Price ?? 0,
                Quantity = quantity,

            };
            cart.Add(item);
        }
        else
        {
            item.Quantity += quantity;
        }
        var productStock = _context.Products.SingleOrDefault(p => p.Slug == slug);
        if (productStock != null && item.Quantity > productStock.Quantity)
        {
            return Json(new { success = false, message = $"Không thể thêm số lượng vượt quá tồn kho" });
        }
        CartHelper.SetCart(HttpContext.Session, cart);
        return Json(new { success = true, message = "Sản phẩm đã được thêm vào giỏ hàng!" });
    }
    public IActionResult RemoveCartItem(string slug)
    {
        var cart = Carts;
        var item = cart.SingleOrDefault(p => p.Slug == slug);
        if (item != null)
        {
            cart.Remove(item);
            CartHelper.SetCart(HttpContext.Session, cart);
        }
        return RedirectToAction("Cart");
    }
    [HttpPost]
    public async Task<IActionResult> UpdateCart(string slug, int quantity)
    {
        var cart = Carts;
        var item = cart.SingleOrDefault(p => p.Slug == slug);
        if (item is not null)
        {
            var productStock = await _context.Products.SingleOrDefaultAsync(p => p.Slug == slug);
            if (productStock is not null && quantity > productStock.Quantity)
            {
                return Json(new
                {
                    success = false,
                    message = "Không thể cập nhật số lượng vượt quá tồn kho",
                    originalQuantity = item.Quantity // Return the original quantity
                });
            }

            item.Quantity = quantity;
            CartHelper.SetCart(HttpContext.Session, cart);
            return Json(new { success = true });
        }
        return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng" });
    }
    public IActionResult ClearCart()
    {
        CartHelper.ClearCart(HttpContext.Session);
        TempData["success"] = "Đã xoá tất cả sản phẩm trong giỏ hàng.";
        return RedirectToAction("Cart");
    }
    [HttpGet("cart-summary")]
    public IActionResult GetCartSummary()
    {
        var cart = Carts;
        var cartVM = new CartVM
        {
            Quantity = cart.Sum(p => p.Quantity),
            Total = cart.Sum(p => p.Total)
        };

        return PartialView("Components/Cart/Default", cartVM);
    }

}
