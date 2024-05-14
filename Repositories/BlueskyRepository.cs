using Microsoft.Extensions.Logging;
using SpotifyNewReleases.Models;
using FishyFlip.Models;
using FishyFlip;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging.Debug;

namespace SpotifyNewReleases.Repositories;

public class BlueskyRepository : IBlueskyRepository
{
    private readonly ILogger<IBlueskyRepository> _logger;
    private readonly ATProtocol _atProtocol;
    private Result<Session> _sessionResult;

    public BlueskyRepository(ILogger<IBlueskyRepository> logger)
    {
        _logger = logger;
        var debugLog = new DebugLoggerProvider();
        var atProtocolBuilder = new ATProtocolBuilder()
            .EnableAutoRenewSession(true)
            .WithLogger(debugLog.CreateLogger("SNRdebug"));
        _atProtocol = atProtocolBuilder.Build()!;
        this.CreateSession().GetAwaiter().GetResult();
    }

    public async Task PostRelease(Item release)
    {
        _sessionResult.Switch(
        async (session) => await this.Post(release, _atProtocol),
        error => this._logger.LogError("Error on creating session: " + error.ToString()));
    }

    private async Task CreateSession()
    {
        string userName = Environment.GetEnvironmentVariable("blueskyUsername") ?? String.Empty;
        string password = Environment.GetEnvironmentVariable("blueskyPassword") ?? String.Empty;
        _sessionResult = await _atProtocol.Server.CreateSessionAsync(userName, password, CancellationToken.None);
    }

    private string GetPrompt(Item release)
    {
        return $"A new release is out! {release.artists.First().name} - {release.name} - {release.type} released on {release.release_date}";
    }

    private Facet GetLink(Item release)
    {
        int linkLength = release.artists.First().name.Length + release.name.Length + 3;
        int byteStart = 22;
        FacetIndex index = new(byteStart, linkLength + byteStart);
        FacetFeature? link = FacetFeature.CreateLink(release.external_urls?.spotify ?? "");
        return new(index, link);
    }

    private async Task Post(Item release, ATProtocol atProtocol)
    {
        Facet link = this.GetLink(release);
        string prompt = this.GetPrompt(release);
        Result<UploadBlobResponse> blobResult = await this.GetImage(release, atProtocol);
        await blobResult.SwitchAsync(
            async blobResponse =>
            {
                FishyFlip.Models.Image? image = blobResponse.Blob.ToImage();
                Result<CreatePostResponse> postResult = await atProtocol.Repo.CreatePostAsync(
                    prompt,
                    [link],
                    new ImagesEmbed(image, $"{release.artists.First().name} - {release.name}"));
                postResult.Switch(
                  success => _logger.LogInformation($"{release.id} posted in Bluesky"),
                  error => _logger.LogError(error.StatusCode.ToString()));
            },
            async error =>
            {
                _logger.LogError($"Error: {error.StatusCode} {error.Detail}");
            });
    }

    private async Task<Result<UploadBlobResponse>> GetImage(Item release, ATProtocol atProtocol)
    {
        Stream stream = await this.GetStream(release.images.First().url);
        StreamContent content = new(stream);
        content.Headers.ContentLength = stream.Length;
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        return await atProtocol.Repo.UploadBlobAsync(content);
    }

    private async Task<Stream> GetStream(string imageUrl)
    {
        HttpClient httpClient = new HttpClient();
        byte[] imageData = await httpClient.GetByteArrayAsync(imageUrl);
        return new MemoryStream(imageData);
    }
}
