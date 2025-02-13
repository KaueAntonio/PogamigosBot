using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordBot.Worker.Domain.Interfaces;
using DiscordBot.Worker.Domain.Objects;
using System.Collections.Concurrent;
using YoutubeVideoDownloader.Implementations;

namespace DiscordBot.Worker.Implementations.Command
{
    public class YtPlayCommand(IMusicService musicService) : ModuleBase<SocketCommandContext>
    {
        private readonly IMusicService _musicService = musicService;
        private static readonly ConcurrentDictionary<ulong, ServerAudioState> _serverAudioStates = new();

        [Command("play")]
        public async Task YtDownloadCommand([Remainder] string videoUrl)
        {
            var guildId = Context.Guild.Id;
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("Você precisa estar em um canal de voz!");
                return;
            }

            if (GetState().AudioClient == null || GetState().AudioClient.ConnectionState != ConnectionState.Connected)
            {
                GetState().AudioClient = await channel.ConnectAsync();
            }

            _musicService.Add(guildId, videoUrl);

            await ReplyAsync($"Video adicionado.");

            if (!_musicService.GetStatus(guildId))
            {
                _ = Task.Run(PlayQueueAsync);
            }
        }

        [Command("skip")]
        public async Task Skip()
        {
            var guildId = Context.Guild.Id;

            if (_musicService.GetStatus(guildId))
            {
                if (GetState().CurrentFfmpegProcess != null && !GetState().CurrentFfmpegProcess.HasExited)
                {
                    try
                    {
                        GetState().CurrentFfmpegProcess.Kill();
                        await Task.Delay(500);
                        GetState().CurrentFfmpegProcess.Dispose();
                    }
                    catch (Exception ex)
                    {
                        await ReplyAsync($"Erro ao encerrar o processo FFmpeg: {ex.Message}");
                    }
                    finally
                    {
                        GetState().CurrentFfmpegProcess = null;
                    }
                }

                await ReplyAsync("Música skipada.");
                await PlayMusic();
            }
            else
            {
                await ReplyAsync("Sem música na fila.");
            }
        }

        [Command("queue")]
        public async Task ShowQueue()
        {
            var guildId = Context.Guild.Id;

            var musics = _musicService.GetQueue(guildId);

            if (musics.Count == 0)
            {
                await ReplyAsync("Fila de músicas vazia.");
                return;
            }

            await ReplyAsync("Fila de músicas:");

            musics.ForEach(async (x) =>
            {
                await ReplyAsync($"{musics.IndexOf(x) + 1} - {_musicService.GetTitle(x)}");
            });
        }

        private async Task PlayQueueAsync()
        {
            var guildId = Context.Guild.Id;

            _musicService.SetStatus(guildId, true);
            while (_musicService.HasNext(guildId))
            {
                try
                {
                    await PlayMusic();
                }
                catch (Exception ex)
                {
                    await ReplyAsync($"Erro: {ex.Message}");
                }
            }

            _musicService.SetStatus(guildId, false);
            await GetState().AudioClient.StopAsync();
            GetState().AudioClient = null;
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            GetState().CurrentFfmpegProcess = _musicService.CreateStream(path);
            using var outputStream = GetState().CurrentFfmpegProcess.StandardOutput.BaseStream;

            GetState().AudioStream = client.CreatePCMStream(AudioApplication.Mixed);

            try
            {
                await outputStream.CopyToAsync(GetState().AudioStream);
                await GetState().AudioStream.FlushAsync();
            }
            finally
            {
                await GetState().AudioStream.DisposeAsync();
                GetState().AudioStream = null;

                if (GetState().CurrentFfmpegProcess != null && !GetState().CurrentFfmpegProcess.HasExited)
                {
                    try
                    {
                        GetState().CurrentFfmpegProcess.Kill();
                        await Task.Delay(500);
                        GetState().CurrentFfmpegProcess.Dispose();
                    }
                    catch (Exception ex)
                    {
                        await ReplyAsync($"Erro ao encerrar o processo FFmpeg: {ex.Message}");
                    }
                    finally
                    {
                        GetState().CurrentFfmpegProcess = null;
                    }
                }
            }
        }

        private ServerAudioState GetState()
        {
            var guildId = Context.Guild.Id;

            return _serverAudioStates.GetOrAdd(guildId, new ServerAudioState());
        }

        private async Task PlayMusic()
        {
            var guildId = Context.Guild.Id;

            var videoUrl = _musicService.PlayAnother(guildId);
            var video = await _musicService.GetVideoAsync(videoUrl);
            await ReplyAsync($"Tocando {video.Title.Split(".mp3")[0]}.");

            await SendAsync(GetState().AudioClient, video.Path);
        }
    }
}