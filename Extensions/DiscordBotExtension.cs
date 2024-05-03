using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using RunMode = Discord.Commands.RunMode;

namespace SpotifyNewReleases.Extensions;

public static class DiscordBotExtensions
{
    public static IServiceCollection AddDiscordSocketClient(this IServiceCollection services)
    {
        var config = new DiscordSocketConfig()
        {
            LogLevel = LogSeverity.Debug,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
        return services.AddSingleton(config).AddSingleton<DiscordSocketClient>();
    }

    public static IServiceCollection AddDiscordCommandService(this IServiceCollection services)
    {
        var config = new CommandServiceConfig()
        {
            CaseSensitiveCommands = false,
            DefaultRunMode = RunMode.Async
        };
        return services.AddSingleton(config).AddSingleton<CommandService>();
    }
}
