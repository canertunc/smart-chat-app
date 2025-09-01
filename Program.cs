using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;
using RagBasedChatbot.Helpers;  
// using RagBasedChatbot.Data;
// using RagBasedChatbot.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


builder.Services.AddSignalR();

builder.Services.AddHttpClient("LlmClient");
builder.Services.AddHttpClient("GnnClient");

builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection("Llm"));

builder.Services.AddHttpClient();

// ADDED: Audio upload'lar için makul boyut limitleri
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
});

builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

var ffmpegPath = builder.Configuration["Ffmpeg:Path"];
if (!string.IsNullOrWhiteSpace(ffmpegPath))
{
    FfmpegConvert.OverridePath = ffmpegPath;
}

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
