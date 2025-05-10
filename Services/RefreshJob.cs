using Microsoft.Extensions.Hosting;
using SearchService.Repositories;
using SearchService.Clients;

namespace SearchService.Services;

public class RefreshJob : BackgroundService
{
    private readonly CocktailRepository _repo;
    private readonly CocktailServiceClient _client;
    private readonly ILogger<RefreshJob> _log;

    public RefreshJob(CocktailRepository repo,
                      CocktailServiceClient client,
                      ILogger<RefreshJob> log)
    {
        _repo = repo; _client = client; _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await _repo.ReloadAsync(_client);           // cooldown gestito dentro
            _log.LogInformation("⏰ Background reload done");
            await Task.Delay(TimeSpan.FromHours(1), ct); // intervallo
        }
    }
}
