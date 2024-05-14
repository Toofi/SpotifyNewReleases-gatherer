using SpotifyNewReleases.Models;

namespace SpotifyNewReleases.Repositories;

public interface ISpotifyRepository
{
    public Task<List<Item>> GetAllLatestReleases(SpotifyToken token);
    public Task<SpotifyToken> GetSpotifyToken();
}
