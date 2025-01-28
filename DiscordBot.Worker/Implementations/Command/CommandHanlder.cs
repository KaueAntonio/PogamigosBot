using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Diagnostics;

namespace DiscordBot.Worker.Implementations.Command
{
    public class CommandHandler : ModuleBase<SocketCommandContext>
    {
        [Command("ytPlay")]
        public async Task YtDownloadCommand([Remainder] string videoUrl)
        {
            VideoCommand videoCommand = new();

            var channel = (Context.User as IGuildUser)?.VoiceChannel;

            if (channel == null)
            {
                await ReplyAsync("Você precisa estar em um canal de voz!");
                return;
            }

            var audioClient = await channel.ConnectAsync();


            var video = await Task.Run(() => videoCommand.GetVideoAsync(videoUrl));
            _ = Task.Run(async () =>
            {
                await SendAsync(audioClient, video.Path);
            });

            await ReplyAsync($"Tocando {video.Title}");
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            using var ffmpeg = CreateStream(path);
            using var output = ffmpeg.StandardOutput.BaseStream;
            using var discord = client.CreatePCMStream(AudioApplication.Mixed);

            try 
            {
                await output.CopyToAsync(discord);
            }
            finally 
            { 
                await discord.FlushAsync();
                File.Delete(path);
            }
        }
    }
}
