using Ecommerce_WatchShop.Models;
using Ecommerce_WatchShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_WatchShop.Components
{
    public class MenuBrandViewComponent : ViewComponent
    {
        private readonly DongHoContext _context;
        public MenuBrandViewComponent(DongHoContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuBrand = await _context.Brands.Select(b => new MenuBrandVM
            {
                BrandId = b.BrandId,
                Name = b.Name,
                Slug = b.Slug
            }).ToListAsync();
            return View(menuBrand);
        }
    }
}
