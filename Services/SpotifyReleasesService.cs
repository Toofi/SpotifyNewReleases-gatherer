using SpotifyNewReleases.Models;
using SpotifyNewReleases.Repositories;
using Discord;
using Microsoft.Extensions.Logging;
using SpotifyNewReleases.Exceptions;

namespace SpotifyNewReleases.Services.Implementations;

public class SpotifyReleasesService : ISpotifyReleasesService
{
    private readonly ISpotifyRepository _spotifyRepository;
    private readonly IAlbumsRepository _albumsRepository;
    private readonly IDiscordRepository _discordRepository;
    private readonly IBlueskyRepository _blueskyRepository;
    private readonly ILogger<ISpotifyReleasesService> _logger;

    public SpotifyReleasesService(ISpotifyRepository spotifyRepository,
        IAlbumsRepository albumsRepository,
        IBlueskyRepository blueskyRepository,
        IDiscordRepository discordRepository,
        ILogger<SpotifyReleasesService> logger
        )
    {
        _spotifyRepository = spotifyRepository;
        _albumsRepository = albumsRepository;
        _discordRepository = discordRepository;
        _blueskyRepository = blueskyRepository;
        _logger = logger;
    }

    public async Task ProcessLatestReleases()
    {
        //avoir le token, puis à partir de ça avoir les sorties
        //aller vérifier dans le repo si chaque entrée existe déjà
        //si c'est une nouvelle, l'insérer dans la db
        //et envoyer dans un post bluesky
        SpotifyToken spotifyToken = await _spotifyRepository.GetSpotifyToken();
        if (spotifyToken is null)
        {
            string error = "There is no received token";
            _logger.LogError(error);
            throw new SpotifyTokenException(error);
        }
        await GetLatestReleases(spotifyToken);
    }

    private async Task GetLatestReleases(SpotifyToken token)
    {
        List<Item> releases = await _spotifyRepository.GetLatestReleases(token);
        await _albumsRepository.AddBulkReleases(releases);
        List<Item> distinctReleases = GetDistinctReleases(releases);
        if (distinctReleases is null || distinctReleases.Count == 0)
        {
            string error = "There is no releases";
            _logger.LogError(error);
            throw new ReleasesException(error);
        }
        await this.SendNewReleases(distinctReleases);
    }

    private static List<Item> GetDistinctReleases(List<Item> releases)
    {
        return releases
            .DistinctBy(release => release.id)
            .OrderBy(release => release.release_date)
            .Reverse()
            .ToList();
    }

    private async Task SendNewReleases(List<Item> items)
    {
        uint newReleasesCount = 0;
        foreach (Item release in items)
        {
            if (!await this.IsReleaseAlreadyExisting(release.id))
            {
                newReleasesCount++;
                try
                {
                    await _albumsRepository.AddNewRelease(release);
                    await this.SendReleasesToClients(release);
                }
                catch (Exception exception)
                {
                    _logger.LogError($"Error: {exception.Message}");
                    throw;
                }
            }
        }
        Console.WriteLine($"added {newReleasesCount} new releases.");
    }

    private async Task<bool> IsReleaseAlreadyExisting(string releaseId)
    {
        Item release = await _albumsRepository.GetRelease(releaseId);
        return release is not null;
    }

    private async Task SendReleasesToClients(Item release)
    {

        //thread sleep due to rate limits with bluesky
        Thread.Sleep(750);
        await this.SendToDiscord(release);
        await this.SendToBluesky(release);
    }

    private async Task SendToDiscord(Item release)
    {
        Embed embeddedRelease = new EmbedBuilder()
           .WithAuthor(release.artists.First().name)
           .WithUrl(release.external_urls?.spotify)
           .WithColor(Color.DarkGreen)
           .WithDescription($"type: {release.album_type}")
           .WithTitle(release.name)
           .WithThumbnailUrl(release.images.First().url)
           .WithFooter(release.release_date).Build();
        await this._discordRepository.SendEmbeddedMessageToAllGuildsAsync(embeddedRelease, release.id);
    }

    private async Task SendToBluesky(Item release)
    {
        await this._blueskyRepository.PostRelease(release);
    }
}
