using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SimpleProject.WebAdmin.Components;

[HtmlTargetElement("dropdown")]
public partial class Dropdown : SelectTagHelper
{
    public bool Search { get; set; }
    public string? Url { get; set; }
    public int MinSearch { get; set; } = 1;
    public bool Autocomplete { get; set; }
    public bool? Required { get; set; }

    public Dropdown(IHtmlGenerator generator) : base(generator)
    {
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "select";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("select2", HtmlEncoder.Default);
        if (!string.IsNullOrEmpty(Url) )
        {
            output.Attributes.SetAttribute("data-url", Url);
            if (MinSearch > 0 )
            {
                output.Attributes.SetAttribute("data-minimum-results-for-search", MinSearch.ToString());
            }
        }

        if (!Search && string.IsNullOrEmpty(Url))
        {
            output.Attributes.SetAttribute("data-minimum-results-for-search", "Infinity");
        }

        if (Autocomplete)
        {
            output.Attributes.SetAttribute("data-autocomplete", "true");
        }

        if (Required != null && Required.HasValue)
        {
            output.Attributes.SetAttribute("data-rule-required", "true");
        }

        return base.ProcessAsync(context, output);
    }
}
