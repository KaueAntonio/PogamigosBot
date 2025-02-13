using Discord.Audio;
using System.Diagnostics;

namespace DiscordBot.Worker.Domain.Objects
{
    public class ServerAudioState
    {
        public IAudioClient AudioClient { get; set; }
        public bool IsPlaying { get; set; }
        public Process CurrentFfmpegProcess { get; set; }
        public AudioOutStream AudioStream { get; set; }
    }
}
