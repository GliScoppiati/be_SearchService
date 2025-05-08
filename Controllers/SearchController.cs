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
        var normalizedFilter = Normalize(request.FilterName);

        var cocktails = _cocktailRepo.GetCocktails().Where(c =>
        {
            // Alcoholic check
            if (!alcoholAllowed && c.IsAlcoholic)
                return false;

            return request.FilterType switch
            {
                "ingredients" => _cocktailRepo.GetIngredientMap()
                .Where(m => m.CocktailId == c.CocktailId)
                .Join(_cocktailRepo.GetIngredients(),
                    map => map.IngredientId,
                    ingredient => ingredient.IngredientId,
                    (map, ingredient) => Normalize(ingredient.NormalizedName))
                .Any(n => n.Contains(normalizedFilter)),
                "category" => Normalize(c.Category).Equals(normalizedFilter),
                "glass" => Normalize(c.Glass).Equals(normalizedFilter),
                "alcoholic" => c.IsAlcoholic.ToString().ToLower().Equals(normalizedFilter),
                "cocktail" => Normalize(c.Name).Contains(normalizedFilter),
                _ => false
            };
        })
        .Select(c => new CocktailDto
        {
            CocktailId = c.CocktailId,
            Name = c.Name,
            ImageUrl = c.ImageUrl
        })
        .ToList();

        return Ok(cocktails);
    }
}

public class SearchRequestDto
{
    public string FilterType { get; set; } = "";
    public string FilterName { get; set; } = "";
}
