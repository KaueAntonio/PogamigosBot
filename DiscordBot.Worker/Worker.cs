using Discord.WebSocket;
using Discord;
using Discord.Commands;
using System.Runtime.InteropServices;
using System.Reflection;

namespace DiscordBot.Worker
{
    public class Worker : BackgroundService
    {
        private readonly DiscordSocketClient _client;
        private readonly string _token;
        private readonly CommandService _commandService;

        public Worker(IConfiguration configuration, IServiceProvider provider, CommandService commandService)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All | GatewayIntents.GuildVoiceStates,
                LogLevel = LogSeverity.Info
            });

            _commandService = commandService;

            _token = configuration["secrets:token"];

            _client.Log += LogAsync;
            _client.MessageReceived += HandleMessageAsync;
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);

            if (message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix("+", ref argPos))
            {
                var result = await _commandService.ExecuteAsync(context, argPos, null);
                if (!result.IsSuccess)
                {
                    await context.Channel.SendMessageAsync($"Erro: {result.ErrorReason}");
                }
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            await _client.StopAsync();
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
