using System.Threading;
using SearchService.Clients;
using SearchService.Models;

namespace SearchService.Repositories;

public class CocktailRepository
{
    private readonly ILogger<CocktailRepository> _logger;

    /* Cache corrente */
    private List<Cocktail> _cocktails = new();
    private List<Ingredient> _ingredients = new();
    private List<CocktailIngredient> _ingredientMap = new();

    /* Sincronizzazione e throttling */
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private DateTime _lastReloadUtc = DateTime.MinValue;

    public CocktailRepository(ILogger<CocktailRepository> logger) => _logger = logger;

    public async Task ReloadAsync(CocktailServiceClient client, bool force = false)
    {
        await _gate.WaitAsync();
        try
        {
            if (!force && DateTime.UtcNow - _lastReloadUtc < TimeSpan.FromHours(1))
                return;

            _logger.LogInformation("[SearchService] 🔄 Avvio reload dati da CocktailService…");

            _logger.LogInformation("[SearchService] 📡 Richiesta GET /cocktail");
            var cocktailsTmp = await client.GetCocktailsAsync();
            _logger.LogInformation("[SearchService] 📦 Ricevuti {Count} cocktail", cocktailsTmp.Count);

            _logger.LogInformation("[SearchService] 📡 Richiesta GET /cocktail/ingredients");
            var ingredientsTmp = await client.GetIngredientsAsync();
            _logger.LogInformation("[SearchService] 📦 Ricevuti {Count} ingredienti", ingredientsTmp.Count);

            _logger.LogInformation("[SearchService] 📡 Richiesta GET /cocktail/ingredients-map");
            var ingredientMapTmp = await client.GetIngredientsMapAsync();
            _logger.LogInformation("[SearchService] 📦 Ricevuti {Count} mapping cocktail-ingredienti", ingredientMapTmp.Count);

            if (!cocktailsTmp.Any())
            {
                _logger.LogWarning("[SearchService] ⚠️ Reload annullato: lista cocktail vuota.");
                return;
            }

            _cocktails = cocktailsTmp;
            _ingredients = ingredientsTmp;
            _ingredientMap = ingredientMapTmp;
            _lastReloadUtc = DateTime.UtcNow;

            _logger.LogInformation(
                "[SearchService] ✅ Reload completato – {CountC} cocktail, {CountI} ingredienti, {CountM} mappe.",
                _cocktails.Count, _ingredients.Count, _ingredientMap.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SearchService] ❌ Errore durante il reload; la cache precedente rimane valida.");
        }
        finally
        {
            _gate.Release();
        }
    }

    /* ---- Accessors pubblici ---- */

    public List<Cocktail> GetCocktails() => _cocktails;
    public List<Ingredient> GetIngredients() => _ingredients;
    public List<CocktailIngredient> GetIngredientMap() => _ingredientMap;

    public DateTime LastReloadUtc => _lastReloadUtc;
}
