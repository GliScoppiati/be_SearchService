using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Repositories;
using SearchService.Models;
using System.Security.Claims;

namespace SearchService.Controllers;

[ApiController]
[Route("history")]
[Authorize]
public class HistoryController : ControllerBase
{
    private readonly HistoryRepository _historyRepo;
    private readonly CocktailRepository _cocktailRepo;

    public HistoryController(HistoryRepository historyRepo, CocktailRepository cocktailRepo)
    {
        _historyRepo = historyRepo;
        _cocktailRepo = cocktailRepo;
    }

    [HttpPost]
    public IActionResult AddHistory([FromBody] HistoryRequestDto request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        _historyRepo.Add(new SearchHistory
        {
            UserId = userId,
            CocktailId = request.CocktailId
        });

        return NoContent();
    }

    [HttpGet]
    public IActionResult GetHistory()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var history = _historyRepo.Get(userId);

        var cocktails = _cocktailRepo.GetCocktails()
            .Where(c => history.Any(h => h.CocktailId == c.CocktailId))
            .Select(c => new { cocktailId = c.CocktailId, cocktailName = c.Name, image = c.ImageUrl })
            .ToList();

        return Ok(cocktails);
    }
}

public class HistoryRequestDto
{
    public Guid CocktailId { get; set; }
}
