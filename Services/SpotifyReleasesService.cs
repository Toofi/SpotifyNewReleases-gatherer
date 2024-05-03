using LanguageExt.Common;
using SpotifyNewReleases.Models;
using SpotifyNewReleases.Repositories;
using Discord;
using Unit = LanguageExt.Unit;

namespace SpotifyNewReleases.Services.Implementations;

public class SpotifyReleasesService : ISpotifyReleasesService
{
    private readonly ISpotifyRepository _spotifyRepository;
    private readonly IAlbumsRepository _albumsRepository;
    private readonly IDiscordRepository _discordRepository;
    private readonly IBlueskyRepository _blueskyRepository;

    public SpotifyReleasesService(ISpotifyRepository spotifyRepository,
        IAlbumsRepository albumsRepository,
        IBlueskyRepository blueskyRepository,
        IDiscordRepository discordRepository
        )
    {
        _spotifyRepository = spotifyRepository;
        _albumsRepository = albumsRepository;
        _discordRepository = discordRepository;
        _blueskyRepository = blueskyRepository;
    }

    public async Task ProcessLatestReleases()
    {
        //avoir le token, puis à partir de ça avoir les sorties
        //aller vérifier dans le repo si chaque entrée existe déjà
        //si c'est une nouvelle, l'insérer dans la db
        //et envoyer dans un post bluesky
        Result<SpotifyToken> spotifyTokenResult = await _spotifyRepository.GetSpotifyToken();
        spotifyTokenResult.IfFail(Console.WriteLine);
        spotifyTokenResult.IfSucc(async (token) => await GetLatestReleases(token));
    }

    private async Task GetLatestReleases(SpotifyToken token)
    {
        Result<List<Item>> itemsResult = await _spotifyRepository.GetAllLatestReleases(token);
        itemsResult.IfFail(Console.WriteLine);
        itemsResult.IfSucc(async (items) => await this.SendNewReleases(items));
    }

    private async Task<Result<Unit>> SendNewReleases(List<Item> items)
    {
        foreach (Item release in items)
        {
            if (!await this.IsReleaseAlreadyExisting(release.id))
            {
                Result<Item> addNewReleaseResult = await _albumsRepository.AddNewRelease(release);
                addNewReleaseResult.IfFail(Console.WriteLine);
                addNewReleaseResult.IfSucc(async (unit) => await this.SendReleasesToClients(release));
            }
        }
        return new Result<Unit>();
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
           .WithUrl(release.external_urls.spotify)
           .WithColor(Color.DarkGreen)
           .WithDescription($"type: {release.album_type}")
           .WithTitle(release.name)
           .WithThumbnailUrl(release.images.First().url)
           .WithFooter(release.release_date).Build();
        await this._discordRepository.SendEmbeddedMessageToAllGuildsAsync(embeddedRelease);
    }

    private async Task SendToBluesky(Item release)
    {
        await this._blueskyRepository.PostRelease(release);
    }
}
