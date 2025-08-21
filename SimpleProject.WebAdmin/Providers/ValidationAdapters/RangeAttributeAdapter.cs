using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.WebAdmin.Providers.ValidationAdapters;

public class RangeAttributeAdapter : AttributeAdapterBase<RangeAttribute>
{
    public RangeAttributeAdapter(RangeAttribute attribute, IStringLocalizer? stringLocalizer) : base(attribute, stringLocalizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        if (context.Attributes.ContainsKey("data-val"))
        {
            context.Attributes.Remove("data-val");
        }
        MergeAttribute(context.Attributes, "data-rule-range", "[" + Attribute.Minimum + ", " + Attribute.Maximum + "]");
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        return "";
    }
}
