using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SpotifyNewReleases
{
    public class GetSpotifyNewReleases
    {
        private readonly ILogger _logger;

        public GetSpotifyNewReleases(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetSpotifyNewReleases>();
        }

        [Function("GetSpotifyNewReleases")]
        public void Run([TimerTrigger("0 47 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
