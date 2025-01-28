using Discord.Commands;
using DiscordBot.Worker;
using DiscordBot.Worker.Implementations.Command;
using System.Reflection;
using System.Runtime.InteropServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<CommandHandler>();

builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<CommandService>((provider) =>
{
    var service = new CommandService(new CommandServiceConfig
    {
        DefaultRunMode = RunMode.Async,
        CaseSensitiveCommands = true,
    });

    service.AddModulesAsync(Assembly.GetEntryAssembly(), provider)
        .GetAwaiter().GetResult();
    return service;
});

var host = builder.Build();
host.Run();
