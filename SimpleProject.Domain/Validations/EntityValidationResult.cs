using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Validations;

public class EntityValidationResult
{
    public IList<ValidationResult>? ValidationErrors { get; private set; }

    public bool HasError
    {
        get { return ValidationErrors?.Count > 0; }
    }

    public EntityValidationResult(IList<ValidationResult>? violations = null)
    {
        ValidationErrors = violations;
        if (violations == null)
        {
            ValidationErrors = new List<ValidationResult>();
        }
    }

    public override string ToString()
    {
        if (ValidationErrors == null)
        {
            return string.Empty;
        }
        return string.Join(Environment.NewLine, ValidationErrors.Where(a => !string.IsNullOrEmpty(a.ErrorMessage)).Select(a => a.ErrorMessage));
    }
}
