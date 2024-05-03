using SpotifyNewReleases.Models;

namespace SpotifyNewReleases.Repositories;

public interface IBlueskyRepository
{
    public Task PostRelease(Item release);
}
