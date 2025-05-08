namespace SearchService.Models;

public class SearchHistory
{
    public Guid UserId { get; set; }
    public Guid CocktailId { get; set; }
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
}
