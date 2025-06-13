using Ecommerce_WatchShop.Models;
using Ecommerce_WatchShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Web_Ban_Dong_Ho.Models;

namespace Ecommerce_WatchShop.Controllers
{

    public class ProductController : Controller
    {

        private readonly DongHoContext _context;
        public ProductController(DongHoContext context)
        {

            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? categories = "", string? brands = "", double? minPrice = null, double? maxPrice = null, int page = 1, int? gender = null)
        {
            var pageSize = 5;  // Số sản phẩm mỗi trang
            var products = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower().Trim();
                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(search) ||
                    p.ShortDescription.ToLower().Contains(search));
            }

            // Lọc theo category
            if (!string.IsNullOrEmpty(categories))
            {
                products = products.Where(p => p.Category!.Slug == categories);
            }

            // Lọc theo brand
            if (!string.IsNullOrEmpty(brands))
            {
                products = products.Where(p => p.Brand!.Slug == brands);
            }
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            if (gender is not null)
            {
                products = products.Where(p => p.Gender == (gender.Value == 1 ? 1 : 0));
            }
            // Lấy tổng số sản phẩm sau khi áp dụng các bộ lọc
            var totalProducts = await products.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            // Lấy các sản phẩm cho trang hiện tại
            var result = await products
                .Where(p => p.Deleted == 0)
                .Include(p => p.ProductRatings)
                .Skip((page - 1) * pageSize) // Bỏ qua các sản phẩm của các trang trước
                .Take(pageSize) // Lấy sản phẩm cho trang hiện tại
                .Select(p => new ProductVM
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName!,
                    Image = p.Image ?? "",
                    Price = p.Price,
                    ShortDescription = p.ShortDescription!,
                    ProductRating = p.ProductRatings.Any()
                        ? p.ProductRatings.Average(r => (double)r.Rating!) : 0,
                    Slug = p.Slug
                })
                .ToListAsync();

            // Tạo ViewModel cho phân trang
            var viewModel = new PagedProductListVM
            {
                Products = result,  // Danh sách sản phẩm cho trang hiện tại
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            // Trả về view với ViewModel
            return View(viewModel);
        }
        public async Task<IActionResult> SearchProduct(string? search = "", int page = 1)
        {
            var pageSize = 5;  // Số sản phẩm mỗi trang
            var products = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower().Trim();
                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(search) ||
                    p.ShortDescription.ToLower().Contains(search));
            }
            var totalProducts = await products.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            var result = await products
                .Where(p => p.Status == 1)
                .Include(p => p.ProductRatings)
                .Skip((page - 1) * pageSize) // Bỏ qua các sản phẩm của các trang trước
                .Take(pageSize) // Lấy sản phẩm cho trang hiện tại
                .Select(p => new ProductVM
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName!,
                    Image = p.Image ?? "",
                    Price = p.Price,
                    ShortDescription = p.ShortDescription!,
                    ProductRating = p.ProductRatings.Any()
                        ? p.ProductRatings.Average(r => (double)r.Rating!) : 0,
                    TotalRating = p.ProductRatings.Count,
                }).ToListAsync();
            var viewModel = new PagedProductListVM
            {
                Products = result,  // Danh sách sản phẩm cho trang hiện tại
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };
            return View(viewModel);
        }
        [Route("ProductDetail/{slug}")]
        public async Task<IActionResult> ProductDetail(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }
            var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CustomerId");
            var customerId = customerIdClaim != null ? int.Parse(customerIdClaim.Value) : (int?)null;
            // Lấy sản phẩm, đánh giá, và bình luận từ cơ sở dữ liệu
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductComments).ThenInclude(productComment => productComment.Customer)
                .Include(p => p.ProductRatings)
                .ThenInclude(c => c.Customer)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (product == null) // Kiểm tra sản phẩm tồn tại
            {
                return NotFound();
            }

            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.BrandId == product.BrandId && p.ProductId != product.ProductId)
                .Take(5)
                .ToListAsync();
            // Tạo ViewModel
            var viewModel = new ProductDetailVM
            {

                Product = product,
                RelatedProducts = relatedProducts,
                ProductRating = product.ProductRatings.Any()
                    ? product.ProductRatings.Average(r => (double)r.Rating!) // Tính trung bình điểm đánh giá
                    : 0,
                TotalRating = product.ProductRatings.Count, // Tổng số đánh giá
                Comments = product.ProductComments
                    .Select(c => new ProductCommentVM
                    {
                        CustomerName = c.Customer?.DisplayName ?? "Guest", // Hiển thị tên khách
                        Content = c.Contents,
                        CreatedAt = c.CreatedAt,
                        Rating = product.ProductRatings.FirstOrDefault(r => r.CustomerId == c.CustomerId)?.Rating
                    }).ToList(),
            };

            return View(viewModel); // Trả về View
        }
        [HttpPost]
        [Route("ProductDetail/{id}/AddReview")]
        public IActionResult AddReview(int id, string content, int rating)
        {
            var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CustomerId");
            int? customerId = customerIdClaim != null ? int.Parse(customerIdClaim.Value) : (int?)null;
            // Kiểm tra sản phẩm tồn tại
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            var comment = new ProductComment
            {

                ProductId = id,
                CustomerId = customerId,
                Contents = content,
                CreatedAt = DateTime.Now
            };

            var productRating = new ProductRating
            {
                ProductId = id,
                CustomerId = customerId,
                Rating = rating
            };

            _context.ProductComments.Add(comment);
            _context.ProductRatings.Add(productRating);
            _context.SaveChanges();

            return RedirectToAction("ProductDetail", new { id }); // Quay lại trang chi tiết sản phẩm
        }
        //public IActionResult AddToCart([FromBody] CartRequest request)
        //{
        //    if (request.Slug is null)
        //    {
        //        return BadRequest(new { message = "ID sản phẩm không hợp lệ!" });
        //    }

        //    var customerIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CustomerId");
        //    int? customerId = customerIdClaim != null ? int.Parse(customerIdClaim.Value) : (int?)null;

        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return Json(new { success = false, message = "Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng." });
        //    }

        //    // Tìm sản phẩm trong cơ sở dữ liệu
        //    var product = _context.Products.FirstOrDefault(p => p.Slug == request.Slug);
        //    if (product == null)
        //    {
        //        return BadRequest(new { message = "Sản phẩm không tồn tại!" });
        //    }

        //    // Tìm sản phẩm trong giỏ hàng của khách hàng
        //    var existingCartItem = _context.Carts.FirstOrDefault(c => c.ProductId == request.PrsluoductId && c.CustomerId == customerId);

        //    if (existingCartItem != null)
        //    {
        //        existingCartItem.Quantity++;
        //        if (existingCartItem.Quantity > product.Quantity)
        //        {
        //            existingCartItem.Quantity = product.Quantity ?? 0;
        //        }
        //        _context.Carts.Update(existingCartItem);
        //    }
        //    else
        //    {
        //        var newCartItem = new Cart
        //        {
        //            ProductId = request.ProductId,
        //            CustomerId = customerId,
        //            Quantity = 1,
        //            Price = (decimal?)product.Price
        //        };
        //        _context.Carts.Add(newCartItem);
        //    }

        //    _context.SaveChanges();
        //    return Ok(new { message = "Sản phẩm đã được thêm vào giỏ hàng!", success = true });
        //}


    }
}