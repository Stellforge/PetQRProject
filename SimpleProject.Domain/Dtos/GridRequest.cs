namespace SimpleProject.Domain.Dtos;

public class GridRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Sorting { get; set; }
    public List<GridFilter>? Filters { get; set; }
    public List<string>? Fields { get; set; }
    public string? SessionKey { get; set; }
}

public class GridFilter
{
    public string? Field { get; set; }
    public string? Operant { get; set; }
    public string? Value { get; set; }
}
