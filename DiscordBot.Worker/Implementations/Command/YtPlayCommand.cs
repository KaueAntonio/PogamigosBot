using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordBot.Worker.Domain.Interfaces;
using DiscordBot.Worker.Domain.Objects;
using System.Collections.Concurrent;

namespace DiscordBot.Worker.Implementations.Command
{
    public class YtPlayCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IMusicService _musicService;
        private static readonly ConcurrentDictionary<ulong, ServerAudioState> _serverAudioStates = new();

        public YtPlayCommand(IMusicService musicService)
        {
            _musicService = musicService;
        }

        [Command("play")]
        public async Task PlayAsync([Remainder] string videoUrl)
        {
            var guildId = Context.Guild.Id;
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("Você precisa estar em um canal de voz!");
                return;
            }

            var state = GetState();
            if (state.AudioClient == null || state.AudioClient.ConnectionState != ConnectionState.Connected)
            {
                state.AudioClient = await channel.ConnectAsync();
            }

            _musicService.Add(guildId, videoUrl);
            await ReplyAsync("Música adicionada.");

            if (!_musicService.GetStatus(guildId))
            {
                _ = Task.Run(() => PlayQueueAsync(guildId));
            }
        }

        [Command("skip")]
        public async Task SkipAsync()
        {
            var guildId = Context.Guild.Id;

            if (_musicService.GetStatus(guildId))
            {
                var state = GetState();
                if (state.CurrentFfmpegProcess != null && !state.CurrentFfmpegProcess.HasExited)
                {
                    try
                    {
                        state.CurrentFfmpegProcess.Kill();
                        await Task.Delay(500);
                        state.CurrentFfmpegProcess.Dispose();
                    }
                    catch (Exception ex)
                    {
                        await ReplyAsync($"Erro ao encerrar o processo FFmpeg: {ex.Message}");
                    }
                    finally
                    {
                        state.CurrentFfmpegProcess = null;
                    }
                }

                await ReplyAsync("Música pulada.");
                await PlayMusicAsync(guildId);
            }
            else
            {
                await ReplyAsync("Sem música na fila.");
            }
        }

        [Command("queue")]
        public async Task ShowQueueAsync()
        {
            var guildId = Context.Guild.Id;
            var musics = _musicService.GetQueue(guildId);

            if (musics.Count == 0)
            {
                await ReplyAsync("Fila de músicas vazia.");
                return;
            }

            await ReplyAsync("Fila de músicas:");
            for (int i = 0; i < musics.Count; i++)
            {
                await ReplyAsync($"{i + 1} - {_musicService.GetTitle(musics[i])}");
            }
        }

        private async Task PlayQueueAsync(ulong guildId)
        {
            _musicService.SetStatus(guildId, true);
            while (_musicService.HasNext(guildId))
            {
                try
                {
                    await PlayMusicAsync(guildId);
                }
                catch (Exception ex)
                {
                    await ReplyAsync($"Erro: {ex.Message}");
                }
            }

            _musicService.SetStatus(guildId, false);
            var state = GetState();
            await state.AudioClient.StopAsync();
            state.AudioClient = null;
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            var state = GetState();
            state.CurrentFfmpegProcess = _musicService.CreateStream(path);
            using var outputStream = state.CurrentFfmpegProcess.StandardOutput.BaseStream;
            state.AudioStream = client.CreatePCMStream(AudioApplication.Mixed);

            try
            {
                await outputStream.CopyToAsync(state.AudioStream);
                await state.AudioStream.FlushAsync();
            }
            finally
            {
                await state.AudioStream.DisposeAsync();
                state.AudioStream = null;

                if (state.CurrentFfmpegProcess != null && !state.CurrentFfmpegProcess.HasExited)
                {
                    try
                    {
                        state.CurrentFfmpegProcess.Kill();
                        await Task.Delay(500);
                        state.CurrentFfmpegProcess.Dispose();
                    }
                    catch (Exception ex)
                    {
                        await ReplyAsync($"Erro ao encerrar o processo FFmpeg: {ex.Message}");
                    }
                    finally
                    {
                        state.CurrentFfmpegProcess = null;
                    }
                }
            }
        }

        private ServerAudioState GetState()
        {
            var guildId = Context.Guild.Id;
            return _serverAudioStates.GetOrAdd(guildId, new ServerAudioState());
        }

        private async Task PlayMusicAsync(ulong guildId)
        {
            var videoUrl = _musicService.PlayAnother(guildId);
            var video = await _musicService.GetVideoAsync(videoUrl);
            await ReplyAsync($"Tocando {video.Title.Split(".mp3")[0]}");

            await SendAsync(GetState().AudioClient, video.Path);
        }
    }
}