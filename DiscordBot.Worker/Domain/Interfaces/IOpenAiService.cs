namespace DiscordBot.Worker.Domain.Interfaces
{
    public interface IOpenAiService
    {
        Task<string> AnswerAsync(string message);
        Task<byte[]> GenerateImageAsync(string prompt);
    }
}
