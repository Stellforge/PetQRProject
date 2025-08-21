using SimpleProject.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SimpleProject.Domain.Validations;

public partial class PasswordAttribute : ValidationAttribute
{
    public PasswordScore Score { get; set; } = PasswordScore.Medium;

    public PasswordAttribute() : base()
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = value as string;
        if (!string.IsNullOrEmpty(password))
        {

            PasswordScore score = GetPasswordScore(password);
            if ((int)Score > (int)score)
            {
                if (Score == PasswordScore.VeryWeak)
                {
                    return new ValidationResult(string.Format("{0} en az 4 karakter uzunluğunda olmalıdır.", validationContext.DisplayName));
                }
                else if (Score == PasswordScore.Weak)
                {
                    return new ValidationResult(string.Format("{0} en az 8 karakter uzunluğunda olmalıdır.", validationContext.DisplayName));
                }
                else if (Score == PasswordScore.Medium)
                {
                    return new ValidationResult(string.Format("{0} en az 8 karakter uzunluğunda, en az 1 küçük, 1 büyük harf ve rakam içermelidir.", validationContext.DisplayName));
                }
                else if (Score == PasswordScore.Strong)
                {
                    return new ValidationResult(string.Format("{0} en az 12 karakter uzunluğunda, en az 1 küçük, 1 büyük harf ve rakam içermelidir.", validationContext.DisplayName));
                }
                return new ValidationResult(string.Format("{0} en az 12 karakter uzunluğunda, en az 1 küçük, 1 büyük harf, 1 özel karakter ve rakam içermelidir.", validationContext.DisplayName));
            }
        }

        return default;
    }

    public static PasswordScore GetPasswordScore(string password)
    {
        int score = 0;
        if (password.Length < 1)
        {
            score = (int)PasswordScore.Blank;
        }
        if (password.Length < 4)
        {
            score = (int)PasswordScore.VeryWeak;
        }
        else
        {
            if (password.Length >= 8)
            {
                score++;
            }
            if (password.Length >= 12)
            {
                score++;
            }
            if (NumberOnlyRegex().IsMatch(password))
            {
                score++;
            }
            if (LowerUpperCaseRegex().IsMatch(password))
            {
                score++;
            }
            if (SpecialLetterRegex().IsMatch(password))
            {
                score++;
            }
        }

        return (PasswordScore)score;
    }

    [GeneratedRegex("[0-9]+(\\.[0-9][0-9]?)?", RegexOptions.IgnoreCase)]
    private static partial Regex NumberOnlyRegex(); //number only //"^\d+$" if you need to match more than one digit.

    [GeneratedRegex("^(?=.*[a-z])(?=.*[A-Z]).+$", RegexOptions.IgnoreCase)]
    private static partial Regex LowerUpperCaseRegex(); //both, lower and upper case

    [GeneratedRegex("[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]", RegexOptions.IgnoreCase)]
    private static partial Regex SpecialLetterRegex(); //^[A-Z]+$
}
