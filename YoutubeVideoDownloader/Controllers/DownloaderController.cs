using Microsoft.AspNetCore.Mvc;
using YoutubeVideoDownloader.Domain.Interfaces;

namespace YoutubeVideoDownloader.Controllers
{
    public static class DownloaderController
    {
        public static void MapDownloadControllers(this WebApplication app)
        {
            var group = app.MapGroup("yt");

            group.MapGet("download", async ([FromQuery] string url, IDownloader _downloader) =>
            {
                var video = await _downloader.Download(url);

                return Results.File(video.File, "application/octet-stream", video.Title);
            });
        }
    }
}
