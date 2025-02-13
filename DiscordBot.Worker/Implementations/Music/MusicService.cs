using DiscordBot.Worker.Domain.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using YoutubeVideoDownloader.Domain.Objects;
using YoutubeVideoDownloader.Implementations;

namespace DiscordBot.Worker.Implementations.Music
{
    public class MusicService : IMusicService
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<string>> _musicQueues = [];
        private readonly Dictionary<string, string> _musicNames = [];
        private readonly ConcurrentDictionary<ulong, bool> _playingStates = new();

        public async void Add(ulong guildId, string music)
        {
            var downloader = new Downloader();
            var queue = _musicQueues.GetOrAdd(guildId, new ConcurrentQueue<string>());

            queue.Enqueue(music);

            var title = await downloader.GetVideoTitleFromUrl(music);
            _musicNames.Add(music, title);

            if (!_playingStates.TryGetValue(guildId, out var isPlaying) || !isPlaying)
            {
                await Task.Run(async () =>
                {
                    await downloader.Download(music);
                });
            }
        }

        public List<string> GetQueue(ulong guildId)
        {
            if (_musicQueues.TryGetValue(guildId, out var queue))
            {
                return [..queue];
            }

            return [];
        }

        public string PlayAnother(ulong guildId)
        {
            if (_musicQueues.TryGetValue(guildId, out var queue))
            {
                if (queue.TryDequeue(out var music))
                {
                    return music;
                }
            }

            throw new Exception("Nenhuma música na fila");
        }

        public bool HasNext(ulong guildId)
        {
            if (_musicQueues.TryGetValue(guildId, out var queue))
            {
                return !queue.IsEmpty;
            }

            return false;
        }

        public bool GetStatus(ulong guildId)
        {
            return _playingStates.TryGetValue(guildId, out var isPlaying) && isPlaying;
        }

        public void SetStatus(ulong guildId, bool isPlaying)
        {
            _playingStates[guildId] = isPlaying;
        }

        public async Task<YtVideo> GetVideoAsync(string url)
        {
            var downloader = new Downloader();
            return await downloader.Download(url);
        }

        public string GetTitle(string key)
        {
            _musicNames.TryGetValue(key, out var name);

            return name;
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
                    CreateNoWindow = true,
                }
            };

            ffmpegProcess.Start();
            return ffmpegProcess;
        }
    }
}