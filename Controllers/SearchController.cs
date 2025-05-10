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
        return string.IsNullOrWhiteSpace(input)
            ? string.Empty
            : input.Trim().ToLowerInvariant();
    }

    [HttpGet("filters")]
    public IActionResult GetFilters([FromQuery] string filterType)
    {
        switch (filterType)
        {
            case "ingredients":
            {
                var ingredients = _cocktailRepo.GetIngredients()
                    .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                    .Select(i => new { name = i.Name })
                    .Distinct();
                return Ok(ingredients);
            }

            case "category":
            {
                var categories = _cocktailRepo.GetCocktails()
                    .Select(c => c.Category)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .Select(name => new { name });
                return Ok(categories);
            }

            case "glass":
            {
                var glasses = _cocktailRepo.GetCocktails()
                    .Select(c => c.Glass)
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                    .Distinct()
                    .Select(name => new { name });
                return Ok(glasses);
            }

            case "alcoholic":
            {
                var options = new[]
                {
                    new { name = "true" },
                    new { name = "false" }
                };
                return Ok(options);
            }

            default:
                return BadRequest("Invalid filter type");
        }
    }

    [HttpPost]
    public IActionResult Search([FromBody] SearchRequestDto request)
    {
        // Verifica claim alcoholAllowed
        var alcoholAllowed = User.Claims
            .FirstOrDefault(c => c.Type == "alcoholAllowed")
            ?.Value
            .Equals("true", StringComparison.OrdinalIgnoreCase) == true;

        // Validazione input
        if (request.Filters is null || request.Filters.Count == 0)
            return BadRequest("At least one filter is required.");

        // Preleva cache per evitare multiple enumerazioni
        var allCocktails   = _cocktailRepo.GetCocktails();
        var allIngredients = _cocktailRepo.GetIngredients();
        var allMaps        = _cocktailRepo.GetIngredientMap();

        var results = allCocktails
            .Where(c =>
            {
                // Escludi alcolici se non consentiti
                if (!alcoholAllowed && c.IsAlcoholic)
                    return false;

                // Applica tutti i filtri (AND)
                foreach (var filter in request.Filters)
                {
                    var normalizedName = Normalize(filter.FilterName);

                    bool isMatch = filter.FilterType switch
                    {
                        "ingredients" => allMaps
                            .Where(m => m.CocktailId == c.CocktailId)
                            .Join(allIngredients,
                                  map => map.IngredientId,
                                  ing => ing.IngredientId,
                                  (map, ing) => Normalize(ing.Name))
                            .Any(n => n.Contains(normalizedName, StringComparison.OrdinalIgnoreCase)),

                        "category"   => Normalize(c.Category).Equals(normalizedName),
                        "glass"      => Normalize(c.Glass).Equals(normalizedName),
                        "alcoholic"  => c.IsAlcoholic
                                          ? normalizedName == "true"
                                          : normalizedName == "false",
                        "cocktail"   => Normalize(c.Name).Contains(normalizedName),
                        _             => false
                    };

                    if (!isMatch)
                        return false;
                }

                return true;
            })
            .Select(c => new CocktailDto
            {
                CocktailId = c.CocktailId,
                Name       = c.Name,
                ImageUrl   = c.ImageUrl
            })
            .ToList();

        return Ok(results);
    }
}
