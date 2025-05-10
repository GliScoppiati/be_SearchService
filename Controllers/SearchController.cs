using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Repositories;
using SearchService.Dtos;
using System.Security.Claims;

namespace SearchService.Controllers;

[ApiController]
[Route("cocktails")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly CocktailRepository _cocktailRepo;

    public SearchController(CocktailRepository cocktailRepo)
    {
        _cocktailRepo = cocktailRepo;
    }

    private static string Normalize(string? input)
    {
        return string.IsNullOrWhiteSpace(input) ? "" : input.Trim().ToLowerInvariant();
    }

    [HttpGet("filters")]
    public IActionResult GetFilters([FromQuery] string filterType)
    {
        if (filterType == "ingredients")
            return Ok(_cocktailRepo.GetIngredients().Select(i => new { name = i.Name }));

        if (filterType == "category")
            return Ok(_cocktailRepo.GetCocktails().Select(c => new { name = c.Category }).Distinct());

        if (filterType == "glass")
            return Ok(_cocktailRepo.GetCocktails().Select(c => new { name = c.Glass }).Distinct());

        if (filterType == "alcoholic")
            return Ok(new[] { new { name = "true" }, new { name = "false" } });

        return BadRequest("Invalid filter type");
    }

    [HttpPost]
    public IActionResult Search([FromBody] SearchRequestDto request)
    {
        var alcoholAllowed = User.Claims.FirstOrDefault(c => c.Type == "alcoholAllowed")?.Value == "True";

        // Nessun filtro → 400
        if (request.Filters is null || request.Filters.Count == 0)
            return BadRequest("At least one filter is required.");

        var cocktails = _cocktailRepo.GetCocktails().Where(c =>
        {
            // 1️⃣  Escludi cocktail alcolici se l’utente non li può vedere
            if (!alcoholAllowed && c.IsAlcoholic)
                return false;

            // 2️⃣  Ogni filtro deve combaciare (AND)
            foreach (var f in request.Filters)
            {
                var normalized = Normalize(f.FilterName);

                var match = f.FilterType switch
                {
                    "ingredients" => _cocktailRepo.GetIngredientMap()
                        .Where(m => m.CocktailId == c.CocktailId)
                        .Join(_cocktailRepo.GetIngredients(),
                            map => map.IngredientId,
                            ingredient => ingredient.IngredientId,
                            (map, ingredient) => Normalize(ingredient.NormalizedName))
                        .Any(n => n.Contains(normalized)),

                    "category"  => Normalize(c.Category).Equals(normalized),
                    "glass"     => Normalize(c.Glass).Equals(normalized),
                    "alcoholic" => c.IsAlcoholic.ToString().ToLower().Equals(normalized),
                    "cocktail"  => Normalize(c.Name).Contains(normalized),
                    _           => false
                };

                if (!match) return false;   // un filtro fallisce → cocktail escluso
            }

            return true; // tutti i filtri passano
        })
        .Select(c => new CocktailDto
        {
            CocktailId = c.CocktailId,
            Name       = c.Name,
            ImageUrl   = c.ImageUrl
        })
        .ToList();

        return Ok(cocktails);
    }
}
