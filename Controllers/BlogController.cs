using Ecommerce_WatchShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection.Metadata;

namespace Ecommerce_WatchShop.Controllers
{
    public class BlogController : Controller
    {
        private readonly DongHoContext _context;

        public BlogController(DongHoContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            int pageSize = 5; // Số lượng bài viết trên mỗi trang
            var blogs = _context.Blogs.AsNoTracking();
            if (!string.IsNullOrEmpty(searchString))
            {
                blogs = blogs.Where(b => b.Subject!.Contains(searchString) || b.Contents!.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }
            return View(await PaginatedList<Blog>.CreateAsync(blogs, pageNumber ?? 1, pageSize));
        }
    }
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {

            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);

        }
    }
}
