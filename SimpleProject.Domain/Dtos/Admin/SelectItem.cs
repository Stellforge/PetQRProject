using System.Text.Json.Serialization;

namespace SimpleProject.Domain.Dtos.Admin;
public class SelectItem
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    public SelectItem() { }

    public SelectItem(string? label, string? value)
    {
        Label = label;
        Value = value;
    }
}
