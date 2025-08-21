using SimpleProject.Domain.Dtos.Admin;

namespace SimpleProject.WebAdmin.Components;

public class GridConfig
{
    public string? Name { get; set; }
    public int? PageSize { get; set; } = 50;
    public List<GridColumnConfig>? Columns { get; set; }
    public FilterConfig? FilterConfig { get; set; }
}

public class GridColumnConfig
{
    public string? Field { get; set; }
    //public string? LangKey { get; set; }
    public bool? Orderable { get; set; }
    public string? OrderType { get; set; }
    public bool? Hidden { get; set; }
    public string? Css { get; set; }
    public string? Template { get; set; }
}

public class FilterConfig
{
    public string? Name { get; set; }
    public string? Title { get; set; }
    public bool? ShowLabel { get; set; }
    public int? ColumnCount { get; set; }
    public FilterButtonType? ButtonType { get; set; }
    public List<FilterFieldConfig>? Fields { get; set; }
}

public class FilterFieldConfig
{
    public string? Names { get; set; }
    //public string? LangKey { get; set; }
    public FilterType? Type { get; set; }
    public FilterOperant? Operant { get; set; }
    public int? LgSize { get; set; }
    public int? MdSize { get; set; }
    public List<ListItem>? Items { get; set; }
}