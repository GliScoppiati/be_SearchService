namespace SearchService.Dtos;

public class CocktailIngredientDto
{
    public Guid CocktailIngredientId { get; set; }
    public Guid CocktailId { get; set; }
    public Guid IngredientId { get; set; }
    public string? OriginalMeasure { get; set; }
}
