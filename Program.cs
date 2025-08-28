using Microsoft.EntityFrameworkCore;
// using RagBasedChatbot.Data;
// using RagBasedChatbot.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


builder.Services.AddSignalR();

builder.Services.AddHttpClient("LlmClient");
builder.Services.AddHttpClient("GnnClient");

builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection("Llm"));

// builder.Services.AddHttpClient();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.MapControllerRoute(
    name : "default",
    pattern : "{controller=Home}/{action=Index}/{id?}");

app.MapHub<LlmHub>("/llmHub");
app.MapHub<GnnHub>("/gnnHub"); 


app.Run();
