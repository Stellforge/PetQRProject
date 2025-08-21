using SimpleProject.Domain.Validations;
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Services;

public interface IValidationService : ISingletonService
{
    EntityValidationResult Validate<T>(T entity);
}

public class ValidationService : IValidationService
{
    public ValidationService()
    {
    }

    public EntityValidationResult Validate<T>(T entity)
    {
        var validationResults = new List<ValidationResult>();
        if (entity != null)
        {
            var vc = new ValidationContext(entity, null, null);
            Validator.TryValidateObject(entity, vc, validationResults, true);
        }
        return new EntityValidationResult(validationResults);
    }
}
