using System.Diagnostics;
using System.Text.RegularExpressions;
using YoutubeVideoDownloader.Domain.Interfaces;
using YoutubeVideoDownloader.Domain.Objects;

namespace YoutubeVideoDownloader.Implementations
{
    public class Downloader : IDownloader
    {
        private const string CookiesPath = "cookies.txt";
        private const string AudioFolder = "audios";

        public async Task<YtVideo> Download(string url)
        {
            Directory.CreateDirectory(AudioFolder);

            string videoTitle = await GetVideoTitleFromUrl(url);
            string sanitizedTitle = SanitizeFileName(videoTitle);

            var outputPath = Path.Combine(AudioFolder, sanitizedTitle);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp.exe",
                    Arguments = $"--cookies {CookiesPath} -x --audio-format mp3 -o \"{outputPath}.%(ext)s\" {url} --ffmpeg-location .\\ffmpeg\\bin\\",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (!File.Exists(outputPath + ".mp3"))
                throw new Exception("Erro ao baixar o áudio.");

            var fileBytes = await File.ReadAllBytesAsync(outputPath + ".mp3");

            return new YtVideo()
            {
                Title = sanitizedTitle + ".mp3",
                File = fileBytes,
                Path = outputPath + ".mp3"
            };
        }

        public async Task<string> GetVideoTitleFromUrl(string url)
        {
            using HttpClient client = new();
            string htmlContent = await client.GetStringAsync(url);

            string pattern = @"<meta\s+name=""title""\s+content=""([^""]+)"">";
            Match match = Regex.Match(htmlContent, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                throw new Exception("Título do vídeo não encontrado.");
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