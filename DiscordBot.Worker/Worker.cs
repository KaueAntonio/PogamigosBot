using Discord.WebSocket;
using Discord;
using Discord.Commands;
using System.Reflection;

namespace DiscordBot.Worker
{
    public class Worker : BackgroundService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _services;
        private readonly string _token;

        public Worker(IConfiguration configuration, IServiceProvider services, CommandService commandService)
        {
            _services = services;
            _commandService = commandService;
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All | GatewayIntents.GuildVoiceStates,
                LogLevel = LogSeverity.Info
            });

            _token = configuration["secrets:token"];

            _client.Log += LogAsync;
            _client.MessageReceived += HandleMessageAsync;
            _client.Ready += RegisterCommandsAsync;
        }

        private async Task RegisterCommandsAsync()
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            Console.WriteLine("Comandos registrados.");
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage message || message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);
            int argPos = 0;

            if (message.HasStringPrefix("+", ref argPos))
            {
                var result = await _commandService.ExecuteAsync(context, argPos, _services);
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

            await Task.Delay(-1, stoppingToken);

            await _client.StopAsync();
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
