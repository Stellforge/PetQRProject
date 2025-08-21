using SimpleProject.Domain.Validations;

namespace SimpleProject.Domain;

public class BusException : Exception
{
    public BusException() : base()
    {
    }

    public BusException(string message) : base(message)
    {
    }

    public BusException(Result result) : base(string.Join(Environment.NewLine, result.Errors))
    {
    }

    public BusException(EntityValidationResult result) : base(string.Join(Environment.NewLine, [.. (result.ValidationErrors ?? []).Select(a => a.ErrorMessage).Distinct()]))
    {
    }
}
