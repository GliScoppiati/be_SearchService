namespace SearchService.Dtos;

public class CocktailDto
{
    public Guid CocktailId { get; set; }
    public string Name { get; set; } = "";
    public string? ImageUrl { get; set; }
}
