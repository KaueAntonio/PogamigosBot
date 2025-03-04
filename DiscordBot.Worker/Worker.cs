using Discord.WebSocket;
using Discord;
using Discord.Commands;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace DiscordBot.Worker
{
    public class Worker : BackgroundService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _services;
        private readonly string _token;
        private readonly ConcurrentDictionary<ulong, bool> _activeThreads;

        public Worker(IConfiguration configuration, IServiceProvider services, CommandService commandService)
        {
            _services = services;

            _commandService = commandService;

            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All | GatewayIntents.GuildVoiceStates,
                LogLevel = LogSeverity.Info
            });

            _token = configuration["TOKEN"];

            _activeThreads = new ConcurrentDictionary<ulong, bool>();

            _client.Log += LogAsync;
            _client.MessageReceived += HandleMessageAsync;
            _client.Ready += OnReadyAsync;
        }

        private async Task OnReadyAsync()
        {
            Console.WriteLine($"Bot está conectado como {_client.CurrentUser.Username}");
            Console.WriteLine($"Conectado em {_client.Guilds.Count} servidores:");
            foreach (var guild in _client.Guilds)
            {
                Console.WriteLine($"- {guild.Name} (ID: {guild.Id})");
            }

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
                var guildId = context.Guild?.Id;
                if (guildId.HasValue)
                {
                    if (_activeThreads.TryAdd(guildId.Value, true))
                    {
                        await Task.Run(async () =>
                        {
                            try
                            {
                                var result = await _commandService.ExecuteAsync(context, argPos, _services);
                                if (!result.IsSuccess)
                                {
                                    await context.Channel.SendMessageAsync($"Erro: {result.ErrorReason}");
                                }
                            }
                            finally
                            {
                                _activeThreads.TryRemove(guildId.Value, out _);
                            }
                        });
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync("Já existe uma thread ativa para esta guild.");
                    }
                }
                else
                {
                    var result = await _commandService.ExecuteAsync(context, argPos, _services);
                    if (!result.IsSuccess)
                    {
                        await context.Channel.SendMessageAsync($"Erro: {result.ErrorReason}");
                    }
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