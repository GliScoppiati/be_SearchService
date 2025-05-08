using SearchService.Clients;
using SearchService.Models;

namespace SearchService.Repositories;

public class CocktailRepository
{
    private readonly ILogger<CocktailRepository> _logger;

    private List<Cocktail> _cocktails = new();
    private List<Ingredient> _ingredients = new();
    private List<CocktailIngredient> _ingredientMap = new();

    public CocktailRepository(ILogger<CocktailRepository> logger)
    {
        _logger = logger;
    }

    public async Task ReloadAsync(CocktailServiceClient client)
    {
        _logger.LogInformation("🔄 Inizio reload dati da CocktailService...");

        _cocktails = await client.GetCocktailsAsync();
        _ingredients = await client.GetIngredientsAsync();
        _ingredientMap = await client.GetIngredientsMapAsync();

        _logger.LogInformation("✅ Dati caricati da CocktailService:");
        _logger.LogInformation($"Cocktails: {_cocktails.Count}");
        _logger.LogInformation($"Ingredients: {_ingredients.Count}");
        _logger.LogInformation($"Ingredient Maps: {_ingredientMap.Count}");

        if (_cocktails.Count == 0)
            _logger.LogWarning("⚠️ ATTENZIONE: nessun cocktail trovato!");

        if (_ingredients.Count == 0)
            _logger.LogWarning("⚠️ ATTENZIONE: nessun ingrediente trovato!");

        if (_ingredientMap.Count == 0)
            _logger.LogWarning("⚠️ ATTENZIONE: nessuna mappa ingrediente-cocktail trovata!");
    }

    public List<Cocktail> GetCocktails() => _cocktails;
    public List<Ingredient> GetIngredients() => _ingredients;
    public List<CocktailIngredient> GetIngredientMap() => _ingredientMap;
}
