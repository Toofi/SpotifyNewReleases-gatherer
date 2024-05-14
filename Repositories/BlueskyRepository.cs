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

    public BlueskyRepository(ILogger<IBlueskyRepository> logger)
    {
        _logger = logger;
    }

    public async Task PostRelease(Item release)
    {
        var debugLog = new DebugLoggerProvider();
        var atProtocolBuilder = new ATProtocolBuilder()
            .EnableAutoRenewSession(true)
            .WithLogger(debugLog.CreateLogger("SNRdebug"));
        ATProtocol atProtocol = atProtocolBuilder.Build()!;
        string userName = Environment.GetEnvironmentVariable("blueskyUsername") ?? String.Empty;
        string password = Environment.GetEnvironmentVariable("blueskyPassword") ?? String.Empty;
        Result<Session> createSessionresult = await atProtocol.Server.CreateSessionAsync(userName, password, CancellationToken.None);
        createSessionresult.Switch(
        async (session) => await this.Post(release, atProtocol),
        error => this._logger.LogError(error.ToString()));
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
                    new[] { link },
                    new ImagesEmbed(image, $"{release.artists.First().name} - {release.name}"));
                postResult.Switch(
                  success => _logger.LogInformation($"{release.id} posted"),
                  error => Console.WriteLine(error));
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
