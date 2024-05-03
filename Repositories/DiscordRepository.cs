using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace SpotifyNewReleases.Repositories;

public class DiscordRepository : IDiscordRepository
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IServiceProvider _provider;
    private readonly ILogger<DiscordRepository> _logger;

    public DiscordRepository(IServiceProvider provider, ILogger<DiscordRepository> logger)
    {
        _provider = provider;
        _discordSocketClient = _provider.GetRequiredService<DiscordSocketClient>();
        this.ConfigureDiscord().GetAwaiter().GetResult();
        _logger = logger;
    }

    private async Task ConfigureDiscord()
    {
        string discordToken = Environment.GetEnvironmentVariable("discordBotToken") ?? String.Empty;
        _discordSocketClient.Log += Log;
        await _discordSocketClient.LoginAsync(TokenType.Bot, discordToken);
        await _discordSocketClient.StartAsync();
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    public async Task SendMessageToGuildAsync(ulong guildId, string message)
    {
        SocketGuild guild = _discordSocketClient.GetGuild(guildId);
        SocketTextChannel channel = this.GetFirstTextChannel(guild);
        await channel.SendMessageAsync(message);
    }

    public async Task SendEmbeddedMessageToAllGuildsAsync(Embed embeddedMessage)
    {
        try
        {
            IReadOnlyCollection<SocketGuild> guild = _discordSocketClient.Guilds;
            List<SocketTextChannel> textChannels = this.GetSocketTextChannels(guild);
            foreach (SocketTextChannel textChannel in textChannels)
            {
                await textChannel.SendMessageAsync(embed: embeddedMessage);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("{datetime} - {service} - There is an error in sending embedded message",
                DateTimeOffset.Now,
                nameof(DiscordRepository));
            throw;
        }
    }

    /// <summary>
    /// Get the first textChannel in a given guild.
    /// </summary>
    /// <param name="guild"></param>
    /// <returns></returns>
    private SocketTextChannel GetFirstTextChannel(SocketGuild guild)
    {
        return guild.TextChannels?.FirstOrDefault(channel => channel.GetChannelType() == ChannelType.Text);
    }

    /// <summary>
    /// Get all first textChannels in all subscribed guilds.
    /// </summary>
    /// <param name="guilds"></param>
    /// <returns></returns>
    private List<SocketTextChannel> GetSocketTextChannels(IReadOnlyCollection<SocketGuild> guilds)
    {
        List<SocketTextChannel> channels = new List<SocketTextChannel>();
        foreach (SocketGuild guild in guilds)
        {
            channels.Add(this.GetFirstTextChannel(guild));
        }
        return channels;
    }
}
