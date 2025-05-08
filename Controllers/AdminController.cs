using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Clients;
using SearchService.Repositories;

namespace SearchService.Controllers;

[ApiController]
[Route("cocktails")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly CocktailRepository _cocktailRepo;
    private readonly CocktailServiceClient _client;

    public AdminController(CocktailRepository cocktailRepo, CocktailServiceClient client)
    {
        _cocktailRepo = cocktailRepo;
        _client = client;
    }

    [HttpPost("reload")]
    public async Task<IActionResult> ReloadData()
    {
        await _cocktailRepo.ReloadAsync(_client);
        return Ok("✅ Reload completed.");
    }
}
