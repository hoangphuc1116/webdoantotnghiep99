using Ecommerce_WatchShop.Helper;
using Ecommerce_WatchShop.Models;
using Ecommerce_WatchShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Ban_Dong_Ho.Helper;

namespace Ecommerce_WatchShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DongHoContext _context;

        public List<CartRequest> Carts => CartHelper.GetCart(HttpContext.Session);

        public CheckoutController(DongHoContext context)
        {
            _context = context;
        }

        // Hiển thị form thanh toán
        public IActionResult Index()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                TempData["ShowLoginModal"] = true;
                return RedirectToAction("Index", "Home");
            }

            if (Carts == null || !Carts.Any())
            {
                TempData["error"] = "Giỏ hàng của bạn đang trống";
                return RedirectToAction("Cart", "Cart");
            }

            var viewModel = new CheckoutValidationVM
            {
                CheckoutVM = new CheckoutVM(),
                CartRequest = Carts
            };

            return View(viewModel);
        }

        // Xử lý thanh toán
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutValidationVM model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CustomerId");

            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out int customerId))
            {
                ModelState.AddModelError(string.Empty, "Không thể xác định khách hàng.");
                return View("Index", model);
            }

            var bill = new Bill
            {
                CustomerId = customerId,
                FullName = model.CheckoutVM.FullName,
                Phone = model.CheckoutVM.Phone,
                Email = model.CheckoutVM.Email,
                Address = model.CheckoutVM.Address,
                Province = model.CheckoutVM.Province,
                District = model.CheckoutVM.District,
                Ward = model.CheckoutVM.Ward,
                PaymentMethod = model.CheckoutVM.PaymentMethod,
                Total = model.CheckoutVM.TotalAmount,
                Status = 1,
                OrderDate = DateTime.Now
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.AddAsync(bill);
                await _context.SaveChangesAsync();

                var invoices = model.CartRequest?.Select(item => new Invoice
                {
                    BillId = bill.BillId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = (decimal)item.Price,
                    Total = (decimal)(item.Quantity * item.Price)
                }).ToList();

                if (invoices != null && invoices.Any())
                {
                    await _context.AddRangeAsync(invoices);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                CartHelper.ClearCart(HttpContext.Session);

                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi: {ex.Message}");
                return View("Index", model);
            }
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
