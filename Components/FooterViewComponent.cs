using Ecommerce_WatchShop.Models;
using Ecommerce_WatchShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_WatchShop.Components
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly DongHoContext _context;

        public FooterViewComponent(DongHoContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var footer = await _context.Footers.FirstOrDefaultAsync(x => x.Status);
            var footerLinks = await _context.FooterLinks
                .Where(x => x.Status)
                .OrderBy(x => x.GroupId)
                .ThenBy(x => x.DisplayOrder)
                .ToListAsync();

            var footerVM = new FooterVM
            {
                Footer = footer,
                InformationLinks = footerLinks.Where(x => x.GroupId == 1).ToList(),
                AccountLinks = footerLinks.Where(x => x.GroupId == 2).ToList(),
                CategoryLinks = footerLinks.Where(x => x.GroupId == 3).ToList()
            };

            return View(footerVM);
        }
    }
}