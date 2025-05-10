namespace SearchService.Dtos;

public class SearchRequestDto
{
    public List<FilterDto> Filters { get; set; } = new();
}