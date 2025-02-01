using Discord;
using Discord.Audio;
using Discord.Commands;
using DiscordBot.Worker.Domain.Interfaces;
using System.Diagnostics;

namespace DiscordBot.Worker.Implementations.Command
{
    public class YtPlayCommand : ModuleBase<SocketCommandContext>
    {
        private readonly IMusicService _musicService;
        private static IAudioClient _audioClient;
        private AudioOutStream _audioStream;
        private Process _currentFfmpegProcess;

        public YtPlayCommand(IMusicService musicService)
        {
            _musicService = musicService;
        }

        [Command("play")]
        public async Task YtDownloadCommand([Remainder] string videoUrl)
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("Você precisa estar em um canal de voz!");
                return;
            }

            if (_audioClient == null || _audioClient.ConnectionState != ConnectionState.Connected)
            {
                _audioClient = await channel.ConnectAsync();
            }

            _musicService.Add(videoUrl);
            await ReplyAsync($"Video adicionado.");

            if (!_musicService.GetStatus())
            {
                _ = Task.Run(PlayQueueAsync);
            }
        }

        [Command("skip")]
        public async Task Skip()
        {
            if (_musicService.GetStatus())
            {
                if (_currentFfmpegProcess != null && !_currentFfmpegProcess.HasExited)
                {
                    try
                    {
                        _currentFfmpegProcess.Kill();
                        await Task.Delay(500);
                        _currentFfmpegProcess.Dispose();
                    }
                    catch (Exception ex)
                    {
                        await ReplyAsync($"Erro ao encerrar o processo FFmpeg: {ex.Message}");
                    }
                    finally
                    {
                        _currentFfmpegProcess = null;
                    }
                }

                await PlayMusic();
                await ReplyAsync("Música skipada.");
            }
            else
            {
                await ReplyAsync("Sem música na fila.");
            }
        }

        private async Task PlayQueueAsync()
        {
            _musicService.SetStatus(true);
            while (_musicService.HasNext())
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

            _musicService.SetStatus(false);
            await _audioClient.StopAsync();
            _audioClient = null;
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            _currentFfmpegProcess = _musicService.CreateStream(path);
            using var outputStream = _currentFfmpegProcess.StandardOutput.BaseStream;

            _audioStream = client.CreatePCMStream(AudioApplication.Mixed);

            try
            {
                await outputStream.CopyToAsync(_audioStream);
                await _audioStream.FlushAsync();
            }
            finally
            {
                await _audioStream.DisposeAsync();
                _audioStream = null;

                if (_currentFfmpegProcess != null && !_currentFfmpegProcess.HasExited)
                {
                    try
                    {
                        _currentFfmpegProcess.Kill();
                        await Task.Delay(500);
                        _currentFfmpegProcess.Dispose();
                    }
                    catch (Exception ex)
                    {
                        await ReplyAsync($"Erro ao encerrar o processo FFmpeg: {ex.Message}");
                    }
                    finally
                    {
                        _currentFfmpegProcess = null;
                    }
                }
            }
        }

        private async Task PlayMusic()
        {
            var videoUrl = _musicService.PlayAnother();
            var video = await _musicService.GetVideoAsync(videoUrl);
            await ReplyAsync($"Tocando {video.Title}.");

            await SendAsync(_audioClient, video.Path);
        }
    }
}