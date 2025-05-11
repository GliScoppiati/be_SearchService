using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Repositories;
using SearchService.Clients;

namespace SearchService.Controllers;

[ApiController]
[Authorize(Policy = "AdminOrService")]
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
        Console.WriteLine("🚨 CHIAMATA ARRIVATA AL CONTROLLER /cocktails/reload/now");
        foreach (var c in User.Claims)
            Console.WriteLine($"🔍 CLAIM: {c.Type} = {c.Value}");
            
        var authHeader = Request.Headers["Authorization"].ToString();
        Console.WriteLine("📥 Authorization Header ricevuto: " + authHeader);

        await _repo.ReloadAsync(_client, force: true);
        return Ok(new
        {
            message = "Reload executed successfully.",
            forced = true,
            reloadedAt = DateTime.UtcNow
        });
    }
}
