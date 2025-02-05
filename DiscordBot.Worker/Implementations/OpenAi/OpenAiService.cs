using DiscordBot.Worker.Domain.Interfaces;
using OpenAI.Chat;
using OpenAI.Images;

namespace DiscordBot.Worker.Implementations.OpenAi
{
    public class OpenAiService(IConfiguration configuration) : IOpenAiService
    {
        private readonly ChatClient _chatClient = new(
            model: "gpt-4o-mini",
            apiKey: configuration["Secrets:openAiKey"]
        );

        private readonly ImageClient _imageClient = new(
            model: "dall-e-3",
            apiKey: configuration["Secrets:openAiKey"]
        );

        public async Task<string> AnswerAsync(string message)
        {
            ChatCompletion chatCompletion = await _chatClient.CompleteChatAsync(message);

            return chatCompletion.Content[0].Text;
        }

        public async Task<byte[]> GenerateImageAsync(string prompt)
        {
            GeneratedImage imageResponse = await _imageClient.GenerateImageAsync(
                prompt: prompt
            );

            var httpClient = new HttpClient()
            {
                BaseAddress = imageResponse.ImageUri
            };

            var image = await httpClient.GetByteArrayAsync("");

            return image;
        }
    }
}
