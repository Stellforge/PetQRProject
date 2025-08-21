using SimpleProject.Domain.Validations;

namespace SimpleProject.Domain;

public class Result
{
    public List<string?> Errors { get; set; }
    public List<string?> Warnings { get; set; }
    public bool HasError => Errors.Count > 0;
    public Dictionary<string, object> Extra { get; set; }

    public string GetErrorMessage(string separator = "\r\n")
    {
        return string.Join(separator, Errors.Select(System.Web.HttpUtility.HtmlEncode));
    }

    public Result()
    {
        Errors = [];
        Warnings = [];
        Extra = [];
    }

    public Result(string errorMessage)
    {
        Errors = [errorMessage];
        Warnings = [];
        Extra = [];
    }

    public Result(EntityValidationResult validationResult)
    {
        if (validationResult.ValidationErrors != null)
        {
            Errors = [.. validationResult.ValidationErrors.Select(a => a.ErrorMessage).Distinct()];
        }
        else
        {
            Errors = [];
        }
        Warnings = [];
        Extra = [];
    }

    public Result(Result other)
    {
        Errors = other.Errors;
        Warnings = other.Warnings;
        Extra = other.Extra;
    }

    public T? Value<T>(string key)
    {
        if (Extra != null && Extra.TryGetValue(key, out object? value) && value != null)
        {
            return (T)value;
        }
        return default;
    }
    public void Set<T>(string key, T data)
    {
        if (Extra != null && data != null)
        {
            Extra[key] = data;
        }
    }
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    public Result() : base()
    {
    }

    public Result(string errorMessage) : base(errorMessage)
    {
    }

    public Result(EntityValidationResult validationResult) : base(validationResult)
    {
    }

    public Result(Result other) : base(other)
    {
    }
}
