using Microsoft.EntityFrameworkCore;
using RagBasedChatbot.Data;
using RagBasedChatbot.Models;
using RagBasedChatbot.Models.Nothwind;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<NorthwindDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Northwind")));

var app = builder.Build();

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.MapControllerRoute(
    name : "default",
    pattern : "{controller=Home}/{action=Index}/{id?}");

app.Run();
