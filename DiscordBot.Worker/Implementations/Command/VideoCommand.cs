using YoutubeExplode;
using YoutubeVideoDownloader.Domain.Objects;
using YoutubeVideoDownloader.Implementations;

namespace DiscordBot.Worker.Implementations.Command
{
    public class VideoCommand
    {
        public async Task<YtVideo> GetVideoAsync(string url)
        {
            var client = new YoutubeClient();

            Downloader downloader = new(client);

            var result = await downloader.Download(url);

            return result;
        }
    }
}
