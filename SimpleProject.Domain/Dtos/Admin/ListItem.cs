using System.Text.Json.Serialization;

namespace SimpleProject.Domain.Dtos.Admin;
public class ListItem
{
    [JsonPropertyName("extra")]
    public string? Extra { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    public ListItem() { }

    public ListItem(string? id, string? text)
    {
        Id = id;
        Text = text;
    }

    public ListItem(string? id, string? text, string? extra)
    {
        Id = id;
        Text = text;
        Extra = extra;
    }
}
