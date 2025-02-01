using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Worker.Implementations.Command
{
    public class FloodCommand : ModuleBase<SocketCommandContext>
    {
        [Command("flood")]
        public async Task Send()
        {
            int i = 0;
            while (true)
            {
                if (i == 70) break;
                await ReplyAsync("MANRANDOLA " + (i % 2 == 0 ? "GAY" : "BIXA"));
                i++;
            }

        }
    }
}
