using System.Net.Http.Json;
using SearchService.Models;

namespace SearchService.Clients;

public class CocktailServiceClient
{
    private readonly HttpClient _http;

    public CocktailServiceClient(HttpClient http)
    {
        _http = http;
    }

    // ✅ Cocktail (ESISTE -> /cocktail)
    public async Task<List<Cocktail>> GetCocktailsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<Cocktail>>("cocktail");
            return response ?? new List<Cocktail>();
        }
        catch
        {
            return new List<Cocktail>();
        }
    }

    // ✅ Ingredienti (ESISTE -> /ingredient)
    public async Task<List<Ingredient>> GetIngredientsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<Ingredient>>("ingredients");
            return response ?? new List<Ingredient>();
        }
        catch
        {
            return new List<Ingredient>();
        }
    }

    // ✅ IngredientiCocktail (ESISTE -> /ingredient-cocktail)
    public async Task<List<CocktailIngredient>> GetIngredientsMapAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<CocktailIngredient>>("ingredients-map");
            return response ?? new List<CocktailIngredient>();
        }
        catch
        {
            return new List<CocktailIngredient>();
        }
    }
}
