using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeVideoDownloader.Domain.Interfaces;
using YoutubeVideoDownloader.Domain.Objects;

namespace YoutubeVideoDownloader.Implementations
{
    public class Downloader(YoutubeClient youtubeClient) : IDownloader
    {
        private readonly YoutubeClient _youtubeClient = youtubeClient;

        public async Task<YtVideo> Download(string url)
        {
            var video = await _youtubeClient.Videos.GetAsync(url);

            string sanitizedTitle = SanitizeFileName(video.Title);
            string outputPath = Path.Combine(Environment.CurrentDirectory, "videos", $"{sanitizedTitle}.mp4");

            Directory.CreateDirectory("videos");

            try
            {
                await _youtubeClient.Videos.DownloadAsync(video.Id, outputPath);

                var fileBytes = await File.ReadAllBytesAsync(outputPath);

                return new YtVideo()
                {
                    Title = video.Title + ".mp4",
                    File = fileBytes,
                    Path = outputPath
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erro: {ex.Message}");
                throw;
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }
}
