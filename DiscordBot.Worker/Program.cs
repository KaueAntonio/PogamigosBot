using DiscordBot.Worker.Configurations;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddBotModules();

var host = builder.Build();
host.Run();
