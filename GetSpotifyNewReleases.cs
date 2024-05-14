using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SpotifyNewReleases.Services;

namespace SpotifyNewReleases
{
    public class GetSpotifyNewReleases
    {
        private readonly ILogger _logger;
        private readonly ISpotifyReleasesService _spotifyReleasesService;

        public GetSpotifyNewReleases(ILoggerFactory loggerFactory, ISpotifyReleasesService spotifyReleasesService)
        {
            _logger = loggerFactory.CreateLogger<GetSpotifyNewReleases>();
            _spotifyReleasesService = spotifyReleasesService;
        }

        [Function("GetSpotifyNewReleases")]
        public async Task Run([TimerTrigger("15 27 * * * *")] TimerInfo myTimer)
        {
            await this._spotifyReleasesService.ProcessLatestReleases();
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
