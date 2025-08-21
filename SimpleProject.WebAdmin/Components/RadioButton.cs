using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SimpleProject.WebAdmin.Components;

[HtmlTargetElement("radio", TagStructure = TagStructure.WithoutEndTag)]
public partial class RadioButton : TagHelper
{
    public object? Value { get; set; }
    public Boolean Checked { get; set; }
    public bool ShowLabel { get; set; } = true;
    public string? Label { get; set; }
    public bool Disabled { get; set; }
    public bool Inline { get; set; }

    [HtmlAttributeName("asp-for")]
    public ModelExpression? FieldExpression { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext? ViewContext { get; set; }


    private string Prefix
    {
        get
        {
            if (ViewContext?.ViewData.TemplateInfo != null && !string.IsNullOrEmpty(ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix))
            {
                return ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix + ".";
            }
            return string.Empty;
        }
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var name = GetAttributeValue(output, "name", () => { return ""; });
        var id = GetAttributeValue(output, "id", () => { return ""; });
        if (FieldExpression != null)
        {
            name = GetAttributeValue(output, "name", () => { return Prefix + FieldExpression?.Name; });
            id = GetAttributeValue(output, "id", () => { return NotLetterNumberRegex().Replace(Prefix + FieldExpression?.Name, "_"); });
            output.Attributes.RemoveAll("id");
            output.Attributes.RemoveAll("name");

            if (Value == null && FieldExpression.Model != null)
            {
                Value = FieldExpression.Model;
            }

            if (ShowLabel && string.IsNullOrEmpty(Label))
            {
                Label = FieldExpression.Metadata.DisplayName;
            }
        }

        output.AddClass("form-check", HtmlEncoder.Default);
        output.AddClass("form-control", HtmlEncoder.Default);
        output.AddClass("border-0", HtmlEncoder.Default);
        if (Inline)
        {
            output.AddClass("form-check-inline", HtmlEncoder.Default);
        }

        var input = new TagBuilder("input")
        {
            TagRenderMode = TagRenderMode.SelfClosing
        };
        input.MergeAttribute("type", "radio");
        if (!string.IsNullOrEmpty(name))
        {
            input.MergeAttribute("name", name);
        }
        if (!string.IsNullOrEmpty(id))
        {
            input.MergeAttribute("id", id);
        }
        
        input.MergeAttribute("value", Value?.ToString() ?? "");
        if (Checked)
        {
            input.MergeAttribute("checked", "checked");
        }
        input.AddCssClass("form-check-input");

        var eventAttrs = new List<string>();
        foreach (var item in output.Attributes)
        {
            if (item.Name.StartsWith("on"))
            {
                eventAttrs.Add(item.Name);
            }
        }
        foreach (var item in eventAttrs)
        {
            input.MergeAttribute(item, GetAttributeValue(output, item, () => { return ""; }));
            output.Attributes.RemoveAll(item);
        }

        if (Disabled)
        {
            input.MergeAttribute("disabled", "disabled");
        }

        output.Content.AppendHtml(input);

        if (!string.IsNullOrEmpty(Label) && ShowLabel)
        {
            var label = new TagBuilder("label");
            label.MergeAttribute("for", id);
            label.AddCssClass("form-check-label");
            label.InnerHtml.AppendHtml(Label);

            output.Content.AppendHtml(label);
        }
    }

    private static string? GetAttributeValue(TagHelperOutput output, string name, Func<string> fn)
    {
        var value = string.Empty;
        if (output.Attributes.TryGetAttribute(name, out TagHelperAttribute attribute))
        {
            value = attribute.Value?.ToString();
            output.Attributes.RemoveAll(name);
        }
        else if (fn != null)
        {
            value = fn.Invoke();
        }
        return value;
    }

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NotLetterNumberRegex();
}
