using Microsoft.AspNetCore.RateLimiting;
using YoutubeExplode;
using YoutubeVideoDownloader.Controllers;
using YoutubeVideoDownloader.Domain.Interfaces;
using YoutubeVideoDownloader.Implementations;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddScoped<IDownloader, Downloader>();
builder.Services.AddScoped<YoutubeClient>();

builder.Services.AddRateLimiter(opt =>
{
    opt.AddFixedWindowLimiter("DownloadLimit", limitOpt =>
    {
        limitOpt.PermitLimit = 5;
        limitOpt.Window = TimeSpan.FromMinutes(5);
    });
});

var app = builder.Build();

app.MapDownloadControllers();

app.Run();
