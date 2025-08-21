using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.WebAdmin.Providers.ValidationAdapters;

public class StringLengthAttributeAdapter : AttributeAdapterBase<StringLengthAttribute>
{
    public StringLengthAttributeAdapter(StringLengthAttribute attribute, IStringLocalizer? stringLocalizer) : base(attribute, stringLocalizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        if (context.Attributes.ContainsKey("data-val"))
        {
            context.Attributes.Remove("data-val");
        }
        MergeAttribute(context.Attributes, "data-rule-rangelength", "[" + Attribute.MinimumLength + ", " + Attribute.MaximumLength + "]");
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        return "";
    }
}
