namespace SearchService.Models;

public class CocktailIngredient
{
    public Guid CocktailIngredientId { get; set; }
    public Guid CocktailId { get; set; }
    public Guid IngredientId { get; set; }
    public string? OriginalMeasure { get; set; }
}
