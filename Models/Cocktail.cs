namespace SearchService.Models;

public class Cocktail
{
    public Guid CocktailId { get; set; }
    public string Name { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string? Glass { get; set; }
    public string? Category { get; set; }
    public bool IsAlcoholic { get; set; }
}
