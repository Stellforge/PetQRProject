using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace SimpleProject.WebAdmin.Providers;

public class ValidationAdapterProvider : IValidationAttributeAdapterProvider
{
    public IAttributeAdapter? GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
    {
        if (attribute is RequiredAttribute requiredAttribute)
        {
            return new ValidationAdapters.RequiredAttributeAdapter(requiredAttribute, stringLocalizer);
        }
        else if (attribute is RangeAttribute rangeAttribute)
        {
            return new ValidationAdapters.RangeAttributeAdapter(rangeAttribute, stringLocalizer);
        }
        else if (attribute is StringLengthAttribute stringLengthAttribute)
        {
            return new ValidationAdapters.StringLengthAttributeAdapter(stringLengthAttribute, stringLocalizer);
        }
        else if (attribute is MaxLengthAttribute maxLengthAttribute)
        {
            return new ValidationAdapters.MaxLengthAttributeAdapter(maxLengthAttribute, stringLocalizer);
        }
        else if (attribute is MinLengthAttribute minLengthAttribute)
        {
            return new ValidationAdapters.MinLengthAttributeAdapter(minLengthAttribute, stringLocalizer);
        }
        else if (attribute is EmailAddressAttribute emailAddressAttribute)
        {
            return new ValidationAdapters.EmailAttributeAdapter(emailAddressAttribute, stringLocalizer);
        }
        else if (attribute is UrlAttribute urlAttribute)
        {
            return new ValidationAdapters.UrlAttributeAdapter(urlAttribute, stringLocalizer);
        }
        else if (attribute is CreditCardAttribute creditCardAttribute)
        {
            return new ValidationAdapters.CreditCardAttributeAdapter(creditCardAttribute, stringLocalizer);
        }
        else if (attribute is CompareAttribute compareAttribute)
        {
            return new ValidationAdapters.CompareAttributeAdapter(compareAttribute, stringLocalizer);
        }
        return default;
    }
}