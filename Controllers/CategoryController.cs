using Ecommerce_WatchShop.Models;
using Ecommerce_WatchShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Ban_Dong_Ho.Models;

namespace Ecommerce_WatchShop.Controllers
{

    public class CategoryController : Controller
    {
        private readonly DongHoContext _context;

        public CategoryController(DongHoContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string slug = "")
        {
            var category = _context.Categories.Where(c => c.Slug == slug).FirstOrDefault();
            if (category == null)
            {
                return RedirectToAction("Index");
            }

            var products = _context.Products
                .Where(p => p.CategoryId == category.CategoryId)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductRatings);
            var result = await products.Select(p => new ProductVM
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName!,
                Image = p.ProductImages.FirstOrDefault()!.Image ?? "",
                Price = p.Price,
                ShortDescription = p.ShortDescription!,
                ProductRating = p.ProductRatings.Any()
                    ? p.ProductRatings.Average(r => (double)r.Rating!)
                    : 0,
                TotalRating = p.ProductRatings.Count,
            }).ToListAsync();

            return View(result);
        }
    }
}
