using Microsoft.Extensions.Hosting;
using SearchService.Repositories;
using SearchService.Clients;
using Microsoft.Extensions.Configuration;

namespace SearchService.Services;

public class RefreshJob : BackgroundService
{
    private readonly CocktailRepository _repo;
    private readonly CocktailServiceClient _client;
    private readonly ILogger<RefreshJob> _log;
    private readonly IConfiguration _cfg;
    public RefreshJob(CocktailRepository repo,
                      CocktailServiceClient client,
                      ILogger<RefreshJob> log,
                      IConfiguration cfg)
    {
        _repo = repo;
        _client = client;
        _log = log;
        _cfg = cfg;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await _repo.ReloadAsync(_client);           // cooldown gestito dentro
            _log.LogInformation("⏰ Background reload done");

            // Leggi da env var oppure da config
            var raw = Environment.GetEnvironmentVariable("REFRESH_INTERVAL_MINUTES") ??
                      _cfg["RefreshJob:IntervalMinutes"];

            var minutes = int.TryParse(raw, out var m) && m > 0 ? m : 60;

            _log.LogInformation("⏳ Waiting {Minutes} minutes before next reload", minutes);

            await Task.Delay(TimeSpan.FromMinutes(minutes), ct); // intervallo
        }
    }
}
