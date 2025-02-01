using Discord.Commands;
using DiscordBot.Worker.Domain.Interfaces;
using DiscordBot.Worker.Implementations.Music;
using System.Reflection;

namespace DiscordBot.Worker.Configurations
{
    internal static class DependencyInjectionConfigurations
    {
        public static void AddBotModules(this IServiceCollection services)
        {
            services.AddHostedService<Worker>();

            services.AddSingleton<IMusicService, MusicService>();
            
            services.AddSingleton(provider =>
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
        }
    }
}
