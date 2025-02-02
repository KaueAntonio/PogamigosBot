using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBot.Worker.Domain.Interfaces;

namespace DiscordBot.Worker.Implementations.Command
{
    public class AskCommand(IOpenAiService openAiService) : ModuleBase<SocketCommandContext>
    {
        private readonly IOpenAiService _openAiService = openAiService;

        [Command("ask")]
        public async Task Ask([Remainder] string question)
        {
            var answer = await _openAiService.AnswerAsync(question);
            await ReplyAsync(answer);
        }

        [Command("askImage")]
        public async Task AskImage([Remainder] string question)
        {
            await ReplyAsync("Gerando imagem...");
            byte[] imageBytes = [];
            try
            {
                imageBytes = await _openAiService.GenerateImageAsync(question);
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }

            if (imageBytes is null || imageBytes.Length == 0)
            {
                await ReplyAsync("Não foi possível gerar a imagem.");
                return;
            }

            using var stream = new MemoryStream(imageBytes);

            await ReplyAsync("Aqui está:");
            
            await Context.Channel.SendFileAsync(stream, "image.png");
        }
    }
}
