using System.Diagnostics;
using YoutubeVideoDownloader.Domain.Objects;

namespace DiscordBot.Worker.Domain.Interfaces
{
    public interface IMusicService
    {
        void Add(ulong guildId, string music);
        string PlayAnother(ulong guildId);
        bool GetStatus(ulong guildId);
        void SetStatus(ulong guildId, bool isPlaying);
        bool HasNext(ulong guildId);
        Task<YtVideo> GetVideoAsync(string url);
        Process CreateStream(string path);
        List<string> GetQueue(ulong guildId);
        string GetTitle(string key);
    }
}
