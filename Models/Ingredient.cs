namespace SearchService.Models;

public class Ingredient
{
    public Guid IngredientId { get; set; }
    public string Name { get; set; } = "";
    public string NormalizedName { get; set; } = "";
}
