using DiscordBot.Worker.Domain.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using YoutubeVideoDownloader.Domain.Objects;
using YoutubeVideoDownloader.Implementations;

namespace DiscordBot.Worker.Implementations.Music
{
    public class MusicService : IMusicService
    {
        private readonly ConcurrentQueue<string> _musicQueue = new();
        public bool IsPlaying { get; set; } = false;

        public void Add(string music)
        {
            _musicQueue.Enqueue(music);
        }

        public string PlayAnother()
        {
            return _musicQueue.TryDequeue(out var music) ? music : throw new Exception("Nenhuma música na fila");
        }

        public bool HasNext() => !_musicQueue.IsEmpty;

        public bool GetStatus() => IsPlaying;

        public void SetStatus(bool isPlaying) => IsPlaying = isPlaying;

        public async Task<YtVideo> GetVideoAsync(string url)
        {
            var downloader = new Downloader();
            return await downloader.Download(url);
        }

        public Process CreateStream(string path)
        {
            var ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            ffmpegProcess.Start();
            return ffmpegProcess;
        }
    }
}