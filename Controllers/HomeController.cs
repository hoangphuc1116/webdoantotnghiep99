using Ecommerce_WatchShop.Models;
using Ecommerce_WatchShop.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using Web_Ban_Dong_Ho.Models;


namespace Ecommerce_WatchShop.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DongHoContext _context;

    public HomeController(ILogger<HomeController> logger, DongHoContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var sliders = await _context.Sliders.Where(s => s.Status).OrderBy(s => s.DisplayOrder).ToListAsync();
        return View(sliders);
    }

    public async Task<IActionResult> Introduction()
    {
        var aboutVM = new AboutVM
        {
            About = await _context.Abouts.FirstOrDefaultAsync(),
            Policies = await _context.Policies.ToListAsync()
        };
        return View(aboutVM);
    }
    [HttpGet]
    public IActionResult Contact()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Contact(ContactVM contactVM)
    {
        if (ModelState.IsValid)
        {
            var contact = new Contact
            {
                FullName = string.IsNullOrEmpty(contactVM.FullName) ? "" : contactVM.FullName,
                Email = string.IsNullOrEmpty(contactVM.Email) ? "" : contactVM.Email,
                Subject = string.IsNullOrEmpty(contactVM.Subject) ? "" : contactVM.Subject,
                Note = string.IsNullOrEmpty(contactVM.Note) ? "" : contactVM.Note
            };


            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();


            return RedirectToAction("Index");
        }

        return View("Contact", contactVM);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM registerVM, string? ReturnUrl = null)
    {
        ViewData["ReturnUrl"] = ReturnUrl;

        if (!ModelState.IsValid)
        {
            return PartialView("_RegisterPartial", registerVM);
        }

        var exist = _context.Accounts
            .FirstOrDefault(a => a.Username == registerVM.Username);

        if (exist != null)
        {
            ModelState.AddModelError("Username", "Username đã tồn tại.");
            return PartialView("_RegisterPartial", registerVM);
        }

        var account = new Account
        {
            Username = registerVM.Username,
            Password = registerVM.Password,
            RoleId = 1,
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var customer = new Customer
        {
            AccountId = account.AccountId,
            DisplayName = account.Username
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        TempData["success"] = "Đăng ký thành công";
        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return Json(new { redirectToUrl = ReturnUrl });
        }
        else
        {
            return Json(new { redirectToUrl = Url.Action("Index", "Home") });
        }
    }
    [HttpGet]
    public IActionResult LoginPartial()
    {
        return PartialView("_LoginPartial", new LoginVM());
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM loginVM, string? ReturnUrl = null)
    {
        ViewData["ReturnUrl"] = ReturnUrl;
        if (!ModelState.IsValid)
        {
            return PartialView("_LoginPartial", loginVM);
        }

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == loginVM.Username);
        if (account == null)
        {
            ModelState.AddModelError("Username", "Tài khoản không tồn tại");
            return PartialView("_LoginPartial", loginVM);
        }

        if (account.Password != loginVM.Password)
        {
            ModelState.AddModelError("Password", "Tài khoản hoặc mật khẩu bị sai");
            return PartialView("_LoginPartial", loginVM);
        }

        if (account.RoleId == 2)
        {
            ModelState.AddModelError("Username", "Không có quyền truy cập");
            return PartialView("_LoginPartial", loginVM);
        }
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == account.AccountId);

        var claims = new List<Claim>
        {
            new Claim("AccountId", account.AccountId.ToString()) // Claim cho AccountId
        };
        if (customer != null && customer.CustomerId > 0)
        {
            claims.Add(new Claim("CustomerId", customer.CustomerId.ToString())); // Claim cho CustomerId

        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = loginVM.RememberMe,
            ExpiresUtc = DateTime.Now.AddDays(5),
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        //HttpContext.Session.SetInt32("AccountId", account.AccountId);
        //HttpContext.Session.SetInt32("AccountId",account.Customerid);
        TempData["success"] = "Đăng nhập thành công";
        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return Json(new { redirectToUrl = ReturnUrl });
        }
        else
        {
            return Json(new { redirectToUrl = Url.Action("Index", "Home") });
        }

    }



    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int statuscode)
    {
        if (statuscode == 404)
        {
            return View("404");
        }
        else
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        }
    }
}