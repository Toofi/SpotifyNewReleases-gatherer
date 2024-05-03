using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SpotifyNewReleases.Models;

namespace SpotifyNewReleases.Repositories;

public class AlbumsRepository : IAlbumsRepository
{
    private IMongoCollection<Item> _rawCollection { get; set; }
    private readonly ILogger<IAlbumsRepository> _logger;

    public AlbumsRepository(IMongoClient client, ILogger<IAlbumsRepository> logger)
    {
        IMongoDatabase database = client.GetDatabase("releases");
        _rawCollection = database.GetCollection<Item>("raw");
        _logger = logger;
    }

    public async Task<Result<Item>> AddNewRelease(Item release)
    {
        try
        {
            _logger.LogInformation("{datetime} - {service} - {method} - Inserting new release {id}.",
                DateTimeOffset.Now,
                nameof(AlbumsRepository),
                nameof(AddNewRelease),
                release.id);
            await _rawCollection.InsertOneAsync(release);
            _logger.LogInformation("{datetime} - {service} - {method} - New release {id} inserted.",
                DateTimeOffset.Now,
                nameof(AlbumsRepository),
                nameof(AddNewRelease),
                release.id);
            Console.WriteLine($"{release.id} inserted on db");
            return new Result<Item>(release);
        }
        catch (Exception exception)
        {
            _logger.LogError("{datetime} - {service} - {method} - Release {id} not inserted - {error}",
                DateTimeOffset.Now,
                nameof(AlbumsRepository),
                nameof(AddNewRelease),
                release.id,
                exception.Message);
            return new Result<Item>(exception);
        }
    }

    public async Task<Item> GetRelease(string id)
    {
        var filter = Builders<Item>.Filter.Eq("id", id);
        return await _rawCollection.Find(filter).FirstOrDefaultAsync();
    }
}
