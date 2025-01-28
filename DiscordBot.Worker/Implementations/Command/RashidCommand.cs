using Discord.Commands;
namespace DiscordBot.Worker.Implementations.Command
{
    public class RashidCommand : ModuleBase<SocketCommandContext>
    {
        [Command("rashid")]
        public async Task Send()
        {
            await ReplyAsync("Esse é bixa");
        }
    }
}
