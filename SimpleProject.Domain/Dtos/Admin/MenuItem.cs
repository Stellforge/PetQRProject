using System.Text.Json.Serialization;

namespace SimpleProject.Domain.Dtos.Admin;

public class MenuItem
{
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? LangKey { get; set; }
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public string? Target { get; set; }
    public int Level { get; set; }
    public bool IsHeader { get; set; }
    [JsonIgnore] public bool Selected { get; set; }
}
