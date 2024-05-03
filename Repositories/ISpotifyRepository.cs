using LanguageExt.Common;
using SpotifyNewReleases.Models;

namespace SpotifyNewReleases.Repositories;

public interface ISpotifyRepository
{
    public Task<Result<List<Item>>> GetAllLatestReleases(SpotifyToken token);
    public Task<Result<SpotifyToken>> GetSpotifyToken();
}
