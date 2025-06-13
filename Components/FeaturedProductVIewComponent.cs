using Ecommerce_WatchShop.Models;
using Ecommerce_WatchShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Ban_Dong_Ho.Models;

namespace Ecommerce_WatchShop.Components
{
    public class FeaturedProductVIewComponent : ViewComponent
    {
        private readonly DongHoContext _context;

        public FeaturedProductVIewComponent(DongHoContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            DateTime cutoffDate = DateTime.Now.AddDays(-30);
            var featureProduct = await _context.Products
                .Where(p => p.CreatedAt >= cutoffDate)
                .Select(p => new ProductVM()
                {
                    Slug = p.Slug,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Image = p.Image,
                    ProductRating = p.ProductRatings.Any()
                        ? p.ProductRatings.Average(r => (double)r.Rating!)
                        : 0,
                }).ToListAsync();
            ViewBag.FeaturedProduct = featureProduct;
            return View();
        }
    }
}
