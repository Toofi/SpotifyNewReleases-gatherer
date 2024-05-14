using SpotifyNewReleases.Models;

namespace SpotifyNewReleases.Repositories;

public interface IAlbumsRepository
{
    public Task AddNewRelease(Item entity);
    public Task<Item> GetRelease(string id);
}
