using System.Threading;
using SearchService.Clients;
using SearchService.Models;

namespace SearchService.Repositories;

public class CocktailRepository
{
    private readonly ILogger<CocktailRepository> _logger;

    /* Cache corrente */
    private List<Cocktail>           _cocktails      = new();
    private List<Ingredient>         _ingredients    = new();
    private List<CocktailIngredient> _ingredientMap  = new();

    /* Sincronizzazione e throttling */
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private DateTime _lastReloadUtc = DateTime.MinValue;

    public CocktailRepository(ILogger<CocktailRepository> logger) => _logger = logger;

    public async Task ReloadAsync(CocktailServiceClient client, bool force = false)
    {
        await _gate.WaitAsync();
        try
        {
            /* Cool‑down di 1 ora salvo override “force” */
            if (!force && DateTime.UtcNow - _lastReloadUtc < TimeSpan.FromHours(1))
                return;

            _logger.LogInformation("🔄 Avvio reload dati da CocktailService…");

            /* 1️⃣  Scarica i dati in strutture temporanee */
            var cocktailsTmp     = await client.GetCocktailsAsync();
            var ingredientsTmp   = await client.GetIngredientsAsync();
            var ingredientMapTmp = await client.GetIngredientsMapAsync();

            /* 2️⃣  Evita di svuotare la cache se la sorgente è vuota */
            if (!cocktailsTmp.Any())
            {
                _logger.LogWarning("⚠️ Reload annullato: lista cocktail vuota.");
                return;
            }

            /* 3️⃣  Sostituzione atomica delle liste */
            _cocktails     = cocktailsTmp;
            _ingredients   = ingredientsTmp;
            _ingredientMap = ingredientMapTmp;
            _lastReloadUtc = DateTime.UtcNow;

            /* 4️⃣  Log riepilogativo */
            _logger.LogInformation(
                "✅ Reload completato – {CountC} cocktail, {CountI} ingredienti, {CountM} mappe.",
                _cocktails.Count, _ingredients.Count, _ingredientMap.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Errore durante il reload; la cache precedente rimane valida.");
        }
        finally
        {
            _gate.Release();
        }
    }

    /* ---- Accessors pubblici ---- */

    public List<Cocktail>           GetCocktails()     => _cocktails;
    public List<Ingredient>         GetIngredients()   => _ingredients;
    public List<CocktailIngredient> GetIngredientMap() => _ingredientMap;

    public DateTime LastReloadUtc => _lastReloadUtc;
}
