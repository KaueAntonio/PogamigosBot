using Discord.Commands;
namespace DiscordBot.Worker.Implementations.Command
{
    public class WowHeadCommand : ModuleBase<SocketCommandContext>
    {
        [Command("wowHead-item")]
        public async Task ItemSearch([Remainder] string searchQuery)
        {
            string answer = await WebScrapeAsync(searchQuery);
            await ReplyAsync(answer);
        }

        private static HttpClient sharedClient = new()
        {
            BaseAddress = new Uri("https://www.wowhead.com/"),
        };

        private static async Task<string> WebScrapeAsync(string searchQuery)
        {
            string response = await sharedClient.GetStringAsync($"search?q={searchQuery}");
            Console.WriteLine(response);
            
            return response.Substring(4000, 2000);
        }
    }
}
