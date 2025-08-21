using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace SimpleProject.WebAdmin.Components;

[HtmlTargetElement("file", TagStructure = TagStructure.WithoutEndTag)]
public partial class FileUpload : InputTagHelper
{
    public FileUpload(IHtmlGenerator generator) : base(generator)
    {
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "input";
        output.TagMode = TagMode.SelfClosing;

        output.AddClass("file-upload", HtmlEncoder.Default);
        output.Attributes.RemoveAll("type");
        output.Attributes.Add("type", "text");
        output.Attributes.Add("data-file-upload", "true");

        return base.ProcessAsync(context, output);
    }
}