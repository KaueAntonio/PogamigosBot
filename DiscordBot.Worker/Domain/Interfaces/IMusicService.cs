using System.Diagnostics;
using YoutubeVideoDownloader.Domain.Objects;

namespace DiscordBot.Worker.Domain.Interfaces
{
    public interface IMusicService
    {
        void Add(string music);
        string PlayAnother();
        bool GetStatus();
        void SetStatus(bool isPLaying);
        bool HasNext();
        Task<YtVideo> GetVideoAsync(string url);
        Process CreateStream(string path);
    }
}
