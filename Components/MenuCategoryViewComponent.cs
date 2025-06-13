using Ecommerce_WatchShop.Models;
using Ecommerce_WatchShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_WatchShop.Components
{
    public class MenuCategoryViewComponent : ViewComponent
    {
        private readonly DongHoContext _context;
        public MenuCategoryViewComponent(DongHoContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var category = await _context.Categories.Select(c => new MenuCategoryVM
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                ParentId = c.ParentId,
                Slug = c.Slug,
            }).ToListAsync();
            return View(category);
        }
    }
}
