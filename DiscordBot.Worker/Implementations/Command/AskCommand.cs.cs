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
            try
            {
                var answer = await _openAiService.AnswerAsync(question);
                
                await ReplyAsync(answer);
            }
            catch(Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("askImage")]
        public async Task AskImage([Remainder] string question)
        {
            await ReplyAsync("Gerando imagem...");
            try
            {
                var imageBytes = await _openAiService.GenerateImageAsync(question);

                if (imageBytes is null || imageBytes.Length == 0)
                {
                    throw new Exception("Imagem não gerada.");
                }

                using var stream = new MemoryStream(imageBytes);

                await ReplyAsync("Aqui está:");

                await Context.Channel.SendFileAsync(stream, "image.png");
            }
            catch (Exception ex)
            {
                await ReplyAsync("Não foi possível gerar a imagem:");

                await ReplyAsync(ex.Message);
            }
        }
    }
}
