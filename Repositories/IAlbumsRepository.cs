using LanguageExt.Common;
using SpotifyNewReleases.Models;

namespace SpotifyNewReleases.Repositories;

public interface IAlbumsRepository
{
    public Task<Result<Item>> AddNewRelease(Item entity);

    public Task<Item> GetRelease(string id);
}
