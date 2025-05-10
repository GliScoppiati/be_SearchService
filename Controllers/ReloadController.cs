using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Repositories;
using SearchService.Clients;

namespace SearchService.Controllers;

[ApiController]
[Route("cocktails/reload")]
public class ReloadController : ControllerBase
{
    private readonly CocktailRepository _repo;
    private readonly CocktailServiceClient _client;

    public ReloadController(CocktailRepository repo, CocktailServiceClient client)
    {
        _repo = repo; _client = client;
    }

    [Authorize(Policy = "ServiceOnly")]
    [HttpPost("now")]
    public async Task<IActionResult> ReloadNow()
    {
        await _repo.ReloadAsync(_client, force: true);
        return Ok("✅ Reload eseguito.");
    }
}
