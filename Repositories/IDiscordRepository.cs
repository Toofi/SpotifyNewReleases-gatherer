using Discord;

namespace SpotifyNewReleases.Repositories;

public interface IDiscordRepository
{
    public Task SendMessageToGuildAsync(ulong guildId, string message);
    public Task SendEmbeddedMessageToAllGuildsAsync(Embed embeddedMessage);
}
