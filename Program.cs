using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotifyNewReleases.Extensions;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDiscordSocketClient().AddDiscordCommandService();
        services.AddMongoConnection(Environment.GetEnvironmentVariable("mongoDbConnectionString")!);
        services.AddRepositories();
        services.AddServices();
    })
    .Build();

host.Run();
