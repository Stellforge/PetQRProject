using System.Text.Json;

namespace SimpleProject.Domain;

public class JsonOptions
{
    private static JsonSerializerOptions _default = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public static JsonSerializerOptions Default => _default;
}
