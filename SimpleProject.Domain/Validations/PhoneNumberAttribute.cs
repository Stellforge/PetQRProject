using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SimpleProject.Domain.Validations;

public class PhoneNumberAttribute : ValidationAttribute
{
    private Regex regex = new Regex(@"^5[0-9]{9}$");
    private Regex regexFull = new Regex(@"^5[0-9]{2}-[0-9]{3}-[0-9]{2}-[0-9]{2}$");

    public PhoneNumberAttribute() : base()
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var phone = value as string;
        if (!string.IsNullOrEmpty(phone) && !regexFull.IsMatch(phone))
        {
            return new ValidationResult(string.Format("{0} geçerli deðil.", validationContext.DisplayName));
        }

        return default;
    }
}