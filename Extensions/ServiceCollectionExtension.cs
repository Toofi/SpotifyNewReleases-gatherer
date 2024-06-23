using SpotifyNewReleases.Repositories;
using SpotifyNewReleases.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SpotifyNewReleases.Services;

namespace SpotifyNewReleases.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IAlbumsRepository, AlbumsRepository>()
            .AddScoped<IDiscordRepository, DiscordRepository>()
            .AddScoped<IBlueskyRepository, BlueskyRepository>()
            .AddScoped<ISpotifyRepository, SpotifyRepository>();
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services
            .AddScoped<ISpotifyReleasesService, SpotifyReleasesService>();
    }

    public static IServiceCollection AddMongoConnection(this IServiceCollection services, string connectionString)
    {
        var client = new MongoClient(connectionString);
        try
        {
            client.StartSession();
            var database = client.GetDatabase("releases");
            Console.WriteLine("connection to MongoDb well-established");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Connection failed: {exception.Message}");
            throw;
        }
        return services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
    }
}
