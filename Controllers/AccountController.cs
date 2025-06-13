using Ecommerce_WatchShop.Models;
using Ecommerce_WatchShop.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_WatchShop.Controllers;

public class AccountController : Controller
{

    private readonly DongHoContext _context;
    public AccountController(DongHoContext context)
    {

        _context = context;
    }


    public async Task<IActionResult> Index()
    {
        var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "AccountId");
        if (customerIdClaim == null) return RedirectToAction("Index", "Home");

        int customerId = int.Parse(customerIdClaim.Value);
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == customerId);

        if (customer == null) return NotFound();
        return View(customer);
    }
    public async Task<IActionResult> Edit()
    {
        var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "AccountId");
        if (customerIdClaim == null) return RedirectToAction("Index", "Home");

        int customerId = int.Parse(customerIdClaim.Value);

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == customerId);

        if (customer == null) return NotFound();

        // Tạo CustomerVM và truyền dữ liệu vào từ khách hàng
        var customerVM = new CustomerVM
        {
            FullName = customer.FullName,
            Phone = customer.Phone,
            Address = customer.Address,
            Email = customer.Email,
            DisplayName = customer.DisplayName,
            Dob = customer.Dob,
            Gender = customer.Gender
        };

        return View(customerVM);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomerVM customerVM)
    {
        var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "AccountId");
        if (customerIdClaim == null) return RedirectToAction("Index", "Home");

        int customerId = int.Parse(customerIdClaim.Value);

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == customerId);

        if (customer == null) return NotFound();

        if (!ModelState.IsValid)
        {
            // Nếu ModelState không hợp lệ, trả về lại form để hiển thị lỗi
            return View(customerVM);
        }

        customer.FullName = customerVM.FullName;
        customer.Phone = customerVM.Phone;
        customer.Address = customerVM.Address;
        customer.Email = customerVM.Email;
        customer.DisplayName = customerVM.DisplayName;
        customer.Dob = customerVM.Dob;
        customer.Gender = customerVM.Gender;

        _context.Update(customer);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Account");
    }


    [HttpGet]
    public async Task<IActionResult> Order(int? status)
    {
        ViewBag.Title = "Đơn hàng";

        var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CustomerId");
        if (customerIdClaim == null) return RedirectToAction("Index", "Home");

        int customerId = int.Parse(customerIdClaim.Value);
        Console.WriteLine($"Customer ID: {customerId}");

        var query = _context.Bills.Where(b => b.CustomerId == customerId);

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        var bills = await query.ToListAsync();

        return View(bills);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Order(int id)
    {
        var bill = await _context.Bills.FirstOrDefaultAsync(b => b.BillId == id);

        if (bill == null)
        {
            return NotFound();
        }

        if (bill.Status == 2)
        {
            TempData["error"] = "Đơn hàng đã thanh toán, không thể hủy.";
            return RedirectToAction("Order");
        }

        bill.Status = 3;

        // Cập nhật trạng thái đơn hàng
        _context.Bills.Update(bill);
        await _context.SaveChangesAsync();


        TempData["success"] = "Đơn hàng đã được hủy thành công.";

        return RedirectToAction("Order");
    }


    public IActionResult Favorite()
    {
        //int? customerId = HttpContext.Session.GetInt32("CustomerId");
        var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CustomerId");
        int customerId = int.Parse(customerIdClaim!.Value);
        var favoriteProducts = _context.Favorites
            .Include(f => f.Product)
            .Where(f => f.CustomerId == customerId)
            .Select(f => new FavoriteVM
            {
                ProductId = f.Product.ProductId,
                Name = f.Product.ProductName!,
                Price = f.Product.Price,
                Image = f.Product.Image
            }).ToList();

        return View(favoriteProducts);
    }
    [HttpPost]
    public JsonResult AddToWishlist(int productId)
    {
        var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CustomerId");
        if (customerIdClaim == null)
        {
            return Json(new { success = false, message = "Bạn cần đăng nhập để thêm vào danh sách yêu thích." });
        }
        int customerId = int.Parse(customerIdClaim.Value);
        var existingWishlist = _context.Favorites
            .FirstOrDefault(w => w.CustomerId == customerId && w.ProductId == productId);

        if (existingWishlist != null)
        {
            return Json(new { success = false, message = "Sản phẩm đã có trong danh sách yêu thích!" });
        }
        // Thêm sản phẩm mới vào danh sách yêu thích
        var wishlist = new Favorite
        {
            CustomerId = customerId,
            ProductId = productId
        };
        _context.Favorites.Add(wishlist);
        _context.SaveChanges();

        return Json(new { success = true, message = "Đã thêm vào danh sách yêu thích!" });
    }
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }


}
