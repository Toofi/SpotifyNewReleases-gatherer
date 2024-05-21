using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SpotifyNewReleases.Exceptions;
using SpotifyNewReleases.Models;

namespace SpotifyNewReleases.Repositories;

public class AlbumsRepository : IAlbumsRepository
{
    private IMongoCollection<Item> _rawCollection { get; set; }
    private IMongoCollection<ItemForDump> _dumpCollection { get; set; }
    private readonly ILogger<IAlbumsRepository> _logger;

    public AlbumsRepository(IMongoClient client, ILogger<IAlbumsRepository> logger)
    {
        IMongoDatabase database = client.GetDatabase("releases");
        _rawCollection = database.GetCollection<Item>("raw-dev");
        _dumpCollection = database.GetCollection<ItemForDump>("dump");
        _logger = logger;
    }

    public async Task AddNewRelease(Item release)
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
        }
        catch (Exception exception)
        {
            _logger.LogError("{datetime} - {service} - {method} - Release {id} not inserted - {error}",
                DateTimeOffset.Now,
                nameof(AlbumsRepository),
                nameof(AddNewRelease),
                release.id,
                exception.Message);
            throw new ReleasesException($"{DateTimeOffset.Now} - " +
                $"{nameof(AlbumsRepository)} - " +
                $"{nameof(AddNewRelease)} - " +
                $"Release {release.id} not inserted - {exception.Message}");
        }
    }

    public async Task AddBulkReleases(List<Item> releases)
    {
        List<ItemForDump> forBulk = new();
        foreach (Item release in releases)
        {
            forBulk.Add(new ItemForDump(Guid.NewGuid().ToString(), release));
        }
        await _dumpCollection.InsertManyAsync(forBulk);
        _logger.LogInformation($"Added {releases.Count} releases on bulk collection");
    }

    public async Task<Item> GetRelease(string id)
    {
        var filter = Builders<Item>.Filter.Eq("id", id);
        return await _rawCollection.Find(filter).FirstOrDefaultAsync();
    }

    private record ItemForDump(string Id, Item Item);
}
