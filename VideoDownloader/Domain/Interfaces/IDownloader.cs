using YoutubeVideoDownloader.Domain.Objects;

namespace YoutubeVideoDownloader.Domain.Interfaces
{
    public interface IDownloader
    {
        Task<YtVideo> Download(string url);
        Task<string> GetVideoTitleFromUrl(string url);
    }
}
