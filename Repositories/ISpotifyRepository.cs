using SpotifyNewReleases.Models;

namespace SpotifyNewReleases.Repositories;

public interface ISpotifyRepository
{
    public Task<List<Item>> GetLatestReleases(SpotifyToken token);
    public Task<SpotifyToken> GetSpotifyToken();
}
