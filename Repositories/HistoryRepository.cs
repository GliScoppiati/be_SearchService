using SearchService.Models;

namespace SearchService.Repositories;

public class HistoryRepository
{
    private readonly List<SearchHistory> _history = new();

    public void Add(SearchHistory entry)
    {
        _history.Add(entry);
    }

    public List<SearchHistory> Get(Guid userId)
    {
        return _history.Where(x => x.UserId == userId).ToList();
    }
}
