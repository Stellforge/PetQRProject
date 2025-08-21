using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SimpleProject.Domain.Dtos;
using SimpleProject.Services;
using SimpleProject.Domain;
using Microsoft.AspNetCore.Mvc.Razor;

namespace SimpleProject.WebAdmin.Components;

[HtmlTargetElement("grid")]
public class Grid : TagHelper
{
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    public string? Url { get; set; }
    public int PageSize { get; set; }
    public int Page { get; set; }
    public bool PreventFirstLoad { get; set; }
    public bool NoStore { get; set; }
    public string? FilterSelector { get; set; }
    public string? Config { get; set; }

    public Grid()
    {
        PageSize = 50;
        Page = 1;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var renderId = Domain.Extensions.GenerateKeyword(8);
        var render = !context.Items.ContainsKey("RenderId");
        if (render)
        {
            context.Items.Add("RenderId", renderId);
            context.Items.Add("GridContext_" + renderId, new GridContext());
            context.Items.Add("FilterContext_" + renderId, new FilterContext());
        }
        renderId = context.Items["RenderId"] as string;
        var gridContext = context.Items["GridContext_" + renderId] as GridContext;

        if (string.IsNullOrEmpty(Config))
        {
            Config = GetPageModel()?.Name.TrimEnd([.. "Dto"]);
        }

        var gridConfig = default(GridConfig);
        if (!string.IsNullOrEmpty(Config) && ViewContext != null)
        {
            var gridConfigs = ViewContext.HttpContext.RequestServices.GetRequiredService<IUserAccessor>().Get<List<GridConfig>>("GridSettings");
            gridConfig = gridConfigs?.FirstOrDefault(a => a.Name == Config);
        }

        PageSize = gridConfig?.PageSize ?? PageSize;
        if (context.Items["FilterContext_" + renderId] is FilterContext filterContextForConfig)
        {
            filterContextForConfig.Config = gridConfig?.FilterConfig;
        }

        var table = new TagBuilder("table");
        foreach (var item in output.Attributes)
        {
            table.MergeAttribute(item.Name, item.Value?.ToString(), true);
        }
        output.Attributes.Clear();

        var hashedId = (ViewContext?.RouteData.Values["controller"] + "-" + ViewContext?.RouteData.Values["action"]).GetHashCode();
        var id = table.Attributes.GetValueOrDefault("id");
        if (string.IsNullOrEmpty(id))
        {
            id = "grid" + hashedId;
            if (!NoStore)
            {
                table.MergeAttribute("data-grid-id", id, true);
            }
            table.MergeAttribute("id", id, true);
        }
        else
        {
            table.MergeAttribute("data-grid-id", id + hashedId, true);
        }
        if (gridContext != null)
        {
            gridContext.Id = table.Attributes.GetValueOrDefault("data-grid-id");
        }

        if (gridContext != null && !string.IsNullOrEmpty(gridContext.Id))
        {
            gridContext.Request = ViewContext?.HttpContext.RequestServices.GetRequiredService<IUserAccessor>().Get<GridRequest>(gridContext.Id);
        }

        await output.GetChildContentAsync();

        TagBuilder? filterHtml = null;
        if (context.Items["FilterContext_" + renderId] is FilterContext filterContext && filterContext.HtmlContent != null)
        {
            FilterSelector = "#" + filterContext.Id;
            filterHtml = filterContext.HtmlContent;
        }
        
        if (gridConfig != null && gridConfig.Columns != null && gridConfig.Columns.Count > 0)
        {
            var colums = gridContext?.Columns.Select(a => a.Column).ToList();
            gridContext?.Columns.Clear();

            foreach (var item in gridConfig.Columns)
            {
                var column = colums?.FirstOrDefault(a => a.Field == item.Field);
                new GridColumn()
                {
                    Field = item.Field,
                    //Title = !string.IsNullOrEmpty(item.LangKey) ? Lang.Get(item.LangKey) : column != null ? column.Title : item.Field,
                    Title = column != null ? column.Title : item.Field,
                    Orderable = item.Orderable ?? (column != null && column.Orderable),
                    OrderType = !string.IsNullOrEmpty(item.OrderType) ? item.OrderType : column?.OrderType,
                    Hidden = item.Hidden ?? (column != null && column.Hidden),
                    Css = !string.IsNullOrEmpty(item.Css) ? item.Css : column?.Css,
                    Template = !string.IsNullOrEmpty(item.Template) ? item.Template : column?.Template,
                    OtherAttributes = column?.OtherAttributes?.Select(a => new TagHelperAttribute(a.Name, a.Value)).ToList()
                }.Process(context, output);
            }
        }

        table.AddCssClass("ruleway-grid dataTable");
        table.MergeAttribute("data-url", Url, true);
        table.MergeAttribute("data-page-size", gridContext?.Request != null ? gridContext?.Request.PageSize.ToString() : PageSize.ToString(), true);
        table.MergeAttribute("data-page", gridContext?.Request != null ? gridContext?.Request.Page.ToString() : Page.ToString(), true);
        table.MergeAttribute("data-prevent-load", PreventFirstLoad ? "1" : "0", true);
        if (!string.IsNullOrEmpty(FilterSelector))
        {
            table.Attributes["data-filter"] = FilterSelector;
        }

        var thead = new TagBuilder("thead");
        var tr = new TagBuilder("tr");
        if (gridContext != null)
        {
            foreach (var column in gridContext.Columns)
            {
                tr.InnerHtml.AppendHtml(column.Content);
            }
        }

        thead.InnerHtml.AppendHtml(tr);
        table.InnerHtml.AppendHtml(thead);
        table.InnerHtml.AppendHtml(new TagBuilder("tbody"));

        var card = new TagBuilder("div");
        foreach (var item in output.Attributes)
        {
            card.MergeAttribute(item.Name, item.Value?.ToString(), true);
        }
        output.Attributes.Clear();

        card.AddCssClass("card custom-card");

        var cardBody = new TagBuilder("div");
        cardBody.AddCssClass("card-body p-0");

        var tableResponsive = new TagBuilder("div");
        tableResponsive.AddCssClass("table-responsive");
        tableResponsive.Attributes.Add("style", "min-height: 15rem;");

        tableResponsive.InnerHtml.AppendHtml(table);
        cardBody.InnerHtml.AppendHtml(tableResponsive);
        card.InnerHtml.AppendHtml(cardBody);

        if (gridContext != null)
        {
            if (filterHtml != null)
            {
                gridContext.HtmlContent.Add(filterHtml);
            }
            gridContext.HtmlContent.Add(card);
        }
        if (render)
        {
            output.TagName = "";
            output.TagMode = TagMode.StartTagAndEndTag;
            if (filterHtml != null)
            {
                output.Content.AppendHtml(filterHtml);
            }
            output.Content.AppendHtml(card);
        }
        else
        {
            output.SuppressOutput();
        }
    }

    private Type? GetPageModel()
    {
        if (ViewContext?.View != null && ViewContext.View is RazorView view && view.RazorPage != null)
        {
            var pageType = view.RazorPage.GetType();
            if(pageType.BaseType != null && typeof(RazorPage).IsAssignableFrom(pageType.BaseType) && pageType.BaseType.IsGenericType && pageType.BaseType.GenericTypeArguments.Length > 0)
            {
                return pageType.BaseType.GenericTypeArguments[0];
            }
        }

        return null;
    }
}

[HtmlTargetElement("column", ParentTag = "grid")]
public class GridColumn : TagHelper
{
    public string? Field { get; set; }
    public string? Title { get; set; }
    public bool Orderable { get; set; }
    public string? OrderType { get; set; }
    public bool Hidden { get; set; }
    public string? Css { get; set; }
    public string? Template { get; set; }

    [HtmlAttributeName("for")]
    public ModelExpression? FieldExpression { get; set; }

    public IEnumerable<TagHelperAttribute>? OtherAttributes { get; set; }

    public GridColumn()
    {
        Orderable = true;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var renderId = context.Items["RenderId"] as string;
        var gridContext = context.Items["GridContext_" + renderId] as GridContext;
        if (FieldExpression != null)
        {
            if (string.IsNullOrEmpty(Field))
            {
                Field = FieldExpression.Name;
            }
            if (string.IsNullOrEmpty(Title))
            {
                Title = FieldExpression.Metadata.DisplayName;
            }

            if (string.IsNullOrEmpty(Template))
            {
                var type = FieldExpression.Metadata.ModelType;
                if (type.IsNullableType())
                {
                    type = Nullable.GetUnderlyingType(type)!;
                }
                if (type == typeof(int) || type == typeof(long) || type == typeof(short))
                {
                    Template = "$template.integer";
                }
                else if (type == typeof(decimal) || type == typeof(float))
                {
                    Template = "$template.number";
                }
                else if (type == typeof(DateTime))
                {
                    Template = "$template.dateTime";
                }
                else if (type == typeof(DateOnly))
                {
                    Template = "$template.date";
                }
                else if (type == typeof(bool))
                {
                    Template = "$template.bool";
                }
                else if (type.IsEnum)
                {
                    Template = "$template.badge";
                }
            }
        }

        var column = new TagBuilder("th");
        column.InnerHtml.AppendHtml(Title ?? string.Empty);
        foreach (var item in output.Attributes)
        {
            column.MergeAttribute(item.Name, item.Value?.ToString(), true);
        }

        if (OtherAttributes != null)
        {
            foreach (var item in OtherAttributes)
            {
                column.MergeAttribute(item.Name, item.Value?.ToString(), true);
            }
        }
        
        column.MergeAttribute("data-field", Field);
        column.MergeAttribute("data-orderable", Orderable ? "1" : "0");
        column.MergeAttribute("data-hidden", Hidden ? "1" : "0");
        column.MergeAttribute("data-css", Css);
        if (!string.IsNullOrEmpty(Template))
        {
            column.MergeAttribute("data-template", Template);
        }
        if (Hidden)
        {
            column.AddCssClass("d-none");
        }
        if (gridContext?.Request != null)
        {
            if (!string.IsNullOrEmpty(gridContext.Request.Sorting))
            {
                var sorting = gridContext.Request.Sorting.Split(":");
                if (sorting.First() == Field)
                {
                    column.MergeAttribute("data-order", sorting.Last(), true);
                }
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(OrderType))
            {
                column.MergeAttribute("data-order", OrderType, true);
            }
        }

        gridContext?.Columns.Add((column, this));

        output.SuppressOutput();
    }
}

public class GridContext
{
    public List<TagBuilder> HtmlContent { get; set; }
    public List<(IHtmlContent Content, GridColumn Column)> Columns { get; set; }
    public string? Id { get; set; }
    public GridRequest? Request { get; set; }

    public GridContext()
    {
        HtmlContent = [];
        Columns = [];
    }
}