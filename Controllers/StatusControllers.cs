using Microsoft.AspNetCore.Mvc;
using SearchService.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace SearchService.Controllers;

[ApiController]
[Authorize(Policy = "AdminOrService")]
[Route("status")]
public class StatusController : ControllerBase
{
    private readonly CocktailRepository _repo;
    public StatusController(CocktailRepository repo) => _repo = repo;

    [HttpGet("reload")]
    public IActionResult LastReload() =>
        Ok(new { lastReloadUtc = _repo.LastReloadUtc });   // esposto in read‑only
}
