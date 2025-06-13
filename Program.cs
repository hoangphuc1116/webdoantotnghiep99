using Web_Ban_Dong_Ho;
using Web_Ban_Dong_Ho.Abstractions;
using Web_Ban_Dong_Ho.Helper;
using Web_Ban_Dong_Ho.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DongHoContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Home/Login";
    options.AccessDeniedPath = "/Home/404";
})
.AddCookie("Admin", options =>
 {
     options.LoginPath = "/Admin/Account/Login";
     options.AccessDeniedPath = "/Home/404";
 });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "2").AuthenticationSchemes = new[] { "Admin" });
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

var app = builder.Build();


app.UseStatusCodePagesWithReExecute("/Home/Error", "?statuscode={0}");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();


app.MapStaticAssets();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .RequireAuthorization("Admin")
    .WithStaticAssets();


//app.MapControllerRoute(
//    name: "ProductBrand",
//    pattern: "product-category/brand/{brands}",
//    defaults: new { controller = "Product", action = "ProductList" }
//);

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


//var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DongHoContext>();
//await SeedData.SeedingData(context);

app.Run();
