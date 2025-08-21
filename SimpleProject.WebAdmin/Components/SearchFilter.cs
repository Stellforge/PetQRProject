using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SimpleProject.Domain;
using SimpleProject.Domain.Dtos;
using Microsoft.AspNetCore.Mvc.Razor;
using SimpleProject.Domain.Entities;

namespace SimpleProject.WebAdmin.Components;

[HtmlTargetElement("filters")]
public class SearchFilter : TagHelper
{
    public string? Title { get; set; }
    public bool ShowLabel { get; set; }
    public int ColumnCount { get; set; } = 3;
    public FilterButtonType ButtonType { get; set; }
    public string? Config { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var renderId = Domain.Extensions.GenerateKeyword(8);
        var render = !context.Items.ContainsKey("RenderId");
        if (render)
        {
            context.Items.Add("RenderId", renderId);
            context.Items.Add("FilterContext_" + renderId, new FilterContext());
        }

        if (string.IsNullOrEmpty(Config))
        {
            Config = GetPageModel()?.Name.TrimEnd([.. "Dto"]);
        }

        var filterConfig = default(FilterConfig);
        renderId = context.Items["RenderId"] as string;
        var filterContext = context.Items["FilterContext_" + renderId] as FilterContext;
        if (filterContext != null)
        {
            filterContext.ColumnSize = (int)Math.Ceiling(12M / ColumnCount);
            filterContext.ShowLabel = ShowLabel;
            filterConfig = filterContext.Config;
        }

        //if (filterConfig == null && !string.IsNullOrEmpty(Config) && ViewContext != null)
        //{
        //    var filterConfigs = ViewContext.HttpContext.RequestServices.GetRequiredService<IUserAccessor>().Get<List<FilterConfig>>(Module.Names.GridSettings);
        //    filterConfig = filterConfigs?.FirstOrDefault(a => a.Name == Config);
        //}

        Title = filterConfig?.Title ?? Title;
        ShowLabel = filterConfig?.ShowLabel ?? ShowLabel;
        ColumnCount = filterConfig?.ColumnCount ?? ColumnCount;
        ButtonType = filterConfig?.ButtonType ?? ButtonType;

        var filters = GetFilters(context);
        if (ViewContext != null)
        {
            ViewContext.ViewBag.FilterValues = filters.Where(a => !string.IsNullOrEmpty(a.Field)).ToDictionary(a => a.Field!, a => a.Value);
        }

        output.GetChildContentAsync().Wait();

        if (filterConfig != null && filterConfig.Fields != null && filterConfig.Fields.Count > 0)
        {
            var fields = filterContext?.Fields.Select(a => a.Field).ToList();
            filterContext?.Fields.Clear();

            foreach (var item in filterConfig.Fields)
            {
                var field = fields?.FirstOrDefault(a => a.Names == item.Names);
                var searchField = new SearchFilterField()
                {
                    Names = item.Names,
                    Type = item.Type ?? field?.Type ?? FilterType.TEXT,
                    Operant = item.Operant ?? field?.Operant ?? FilterOperant.CONTAINS,
                    LgSize = item.LgSize ?? field?.LgSize ?? 1,
                    MdSize = item.MdSize ?? field?.MdSize ?? 0,
                    Items = item.Items?.Select(a => new SelectListItem(a.Text, a.Id)).ToList() ?? field?.Items,
                    Label = field?.Label ?? item.Names,
                    //Label = string.IsNullOrEmpty(item.LangKey) ? Lang.Get(item.LangKey) : field?.Label ?? item.Names,
                };

                if (searchField.Type == FilterType.SELECT && !string.IsNullOrEmpty(Config) && (searchField.Items == null || !searchField.Items.Any()))
                {
                    var entityType = AppDomain.CurrentDomain.GetAssemblies().Where(a => !string.IsNullOrEmpty(a.FullName) && a.FullName.StartsWith("SimpleProject")).SelectMany(a => a.GetTypes().Where(b => b.BaseType != null && b.BaseType == typeof(Entity) && b.Name == Config)).FirstOrDefault();
                    if (entityType != null)
                    {
                        var propInfo = entityType.GetProperty(item.Names ?? "");
                        if (propInfo != null)
                        {
                            var propType = propInfo.PropertyType;
                            if (propType.IsValueType())
                            {
                                if (propType.IsNullableType())
                                {
                                    propType = Nullable.GetUnderlyingType(propType)!;
                                }

                                if (propType.IsEnum)
                                {
                                    searchField.Items = new List<SelectListItem>();
                                    foreach (Enum enumItem in Enum.GetValues(propType))
                                    {
                                        ((List<SelectListItem>)searchField.Items).Add(new SelectListItem(enumItem.GetDisplayName(), Convert.ToInt32(enumItem).ToString()));
                                    }
                                }
                            }
                        }
                    }
                }

                searchField.Process(context, output);
            }
        }

        var card = new TagBuilder("div");

        foreach (var item in output.Attributes)
        {
            card.MergeAttribute(item.Name, item.Value?.ToString(), true);
        }
        output.Attributes.Clear();
        card.AddCssClass("card custom-card");

        if (!string.IsNullOrEmpty(Title))
        {
            var cardHeader = new TagBuilder("div");
            cardHeader.AddCssClass("card-header justify-content-between border-bottom-0");

            var cardTitle = new TagBuilder("div");
            cardTitle.AddCssClass("card-title");
            cardTitle.InnerHtml.AppendHtml(Title);

            cardHeader.InnerHtml.AppendHtml(cardTitle);
            card.InnerHtml.AppendHtml(cardHeader);
        }

        var cardBody = new TagBuilder("div");
        cardBody.AddCssClass("card-body p-3");

        var row = new TagBuilder("div");
        row.AddCssClass("row gy-2");

        if (filterContext != null && !filterContext.Visible)
        {
            row.AddCssClass("d-none");
        }

        var id = card.Attributes.GetValueOrDefault("id");
        if (string.IsNullOrEmpty(id))
        {
            id = "filter" + Domain.Extensions.GenerateKeyword(5, true);
            card.MergeAttribute("id", id, true);
        }

        if (filterContext != null)
        {
            filterContext.Id = id;
            for (int i = 0; i < filterContext.Fields.Count; i++)
            {
                if (filterContext.Fields[i].Content is TagBuilder div)
                {
                    if (i == filterContext.Fields.Count - 1 && ButtonType == FilterButtonType.ICON)
                    {
                        var searchButton = new TagBuilder("button");
                        searchButton.AddCssClass("btn btn-secondary search-btn ms-1");
                        searchButton.MergeAttribute("type", "button");
                        searchButton.InnerHtml.AppendHtml(new HtmlString(@"<i class=""ri-search-line""></i>"));
                        if (filterContext.ShowLabel)
                        {
                            searchButton.AddCssClass("mt-3");
                        }
                        div.AddCssClass("d-flex");
                        div.InnerHtml.AppendHtml(searchButton);

                        if (filterContext.ButtonFields.Count != 0)
                        {
                            foreach (var item in filterContext.ButtonFields)
                            {
                                div.InnerHtml.AppendHtml(item);
                            }
                        }
                    }

                    row.InnerHtml.AppendHtml(div);
                }
            }
            if (filterContext.Fields.Count == 0)
            {
                if (filterContext.ButtonFields.Count != 0)
                {
                    foreach (var item in filterContext.ButtonFields)
                    {
                        row.InnerHtml.AppendHtml(item);
                    }
                }
            }

            for (int i = 0; i < filterContext.HtmlFields.Count; i++)
            {
                if (filterContext.HtmlFields[i].Content is TagBuilder div)
                {
                    row.InnerHtml.AppendHtml(div);
                }
            }

            if (ButtonType == FilterButtonType.BUTTON)
            {
                var div = new TagBuilder("div");
                div.AddCssClass("text-end col-12");

                if (filterContext.ButtonFields.Count != 0)
                {
                    foreach (var item in filterContext.ButtonFields)
                    {
                        div.InnerHtml.AppendHtml(item);
                    }
                }

                var searchButton = new TagBuilder("button");
                searchButton.AddCssClass("btn btn-secondary search-btn");
                searchButton.MergeAttribute("type", "button");
                searchButton.InnerHtml.AppendHtml(new HtmlString(@"<i class=""ri-search-line""></i> "));
                searchButton.InnerHtml.Append(" ara");
                div.InnerHtml.AppendHtml(searchButton);


                row.InnerHtml.AppendHtml(div);
            }

            foreach (var item in filterContext.HiddenFields)
            {
                row.InnerHtml.AppendHtml(item);
            }
        }

        cardBody.InnerHtml.AppendHtml(row);
        card.InnerHtml.AppendHtml(cardBody);

        if (render)
        {
            output.TagName = "";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.AppendHtml(card);
        }
        else
        {
            if (filterContext != null)
            {
                filterContext.HtmlContent = card;
            }
            output.SuppressOutput();
        }
    }

    private Type? GetPageModel()
    {
        if (ViewContext?.View != null && ViewContext.View is RazorView view && view.RazorPage != null)
        {
            var pageType = view.RazorPage.GetType();
            if (pageType.BaseType != null && typeof(RazorPage).IsAssignableFrom(pageType.BaseType) && pageType.BaseType.IsGenericType && pageType.BaseType.GenericTypeArguments.Length > 0)
            {
                return pageType.BaseType.GenericTypeArguments[0];
            }
        }

        return null;
    }
    private static List<GridFilter> GetFilters(TagHelperContext context)
    {
        var renderId = context.Items["RenderId"] as string;
        if (context.Items.ContainsKey("GridContext_" + renderId))
        {
            if (context.Items["GridContext_" + renderId] is GridContext gridContext && gridContext.Request != null && gridContext.Request.Filters != null)
            {
                return gridContext.Request.Filters;
            }
        }

        return [];
    }
}

[HtmlTargetElement("field", ParentTag = "filters")]
public class SearchFilterField : TagHelper
{
    public string? Names { get; set; }
    public FilterType Type { get; set; }
    public FilterOperant Operant { get; set; }
    public int LgSize { get; set; } = 1;
    public int MdSize { get; set; }
    public IEnumerable<SelectListItem>? Items { get; set; }
    public string? Label { get; set; }
    public string? PlaceHolder { get; set; }
    public object? Value { get; set; }

    [HtmlAttributeName("for")]
    public ModelExpression? FieldExpression { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var renderId = context.Items["RenderId"] as string;
        var filterContext = context.Items["FilterContext_" + renderId] as FilterContext;

        if (FieldExpression != null)
        {
            Names = FieldExpression.Name;
            if (string.IsNullOrEmpty(Label))
            {
                Label = FieldExpression.Metadata.DisplayName;
            }
        }

        if (Type != FilterType.HIDDEN && filterContext != null)
        {
            filterContext.Visible = true;
        }

        if (string.IsNullOrEmpty(PlaceHolder) && Type != FilterType.HIDDEN)
        {
            if (filterContext != null && filterContext.ShowLabel)
            {
                if (Type == FilterType.SELECT || Type == FilterType.BOOL)
                {
                    PlaceHolder = "Seçiniz";
                }
            }
            else
            {
                PlaceHolder = Label;
                var formatString = "{0} içerisinde ara";
                if (Operant == FilterOperant.GREATER_THAN)
                {
                    formatString = "{0} den büyük";
                }
                else if (Operant == FilterOperant.GREATER_THANEQUAL)
                {
                    formatString = "{0} den büyük eşit";
                }
                else if (Operant == FilterOperant.LESS_THAN)
                {
                    formatString = "{0} den küçük";
                }
                else if (Operant == FilterOperant.LESS_THANEQUAL)
                {
                    formatString = "{0} den küçük eşit";
                }
                if (Type == FilterType.SELECT || Type == FilterType.BOOL)
                {
                    formatString = "{0} seçiniz";
                }
                PlaceHolder = string.Format(formatString, Label);
            }
        }

        if (MdSize == 0)
        {
            MdSize = LgSize * 2;
        }

        var operant = GetOperantString();
        var lgSize = filterContext?.ColumnSize * LgSize > 12 ? 12 : (filterContext?.ColumnSize).GetValueOrDefault() * LgSize;
        var mdSize = filterContext?.ColumnSize * MdSize > 12 ? 12 : (filterContext?.ColumnSize).GetValueOrDefault() * MdSize;

        var value = string.Empty;
        if (Value != null)
        {
            value = Value.ToString();
        }
        else if (ViewContext != null)
        {
            if (ViewContext.ViewBag.FilterValues is Dictionary<string, string?> filters && !string.IsNullOrEmpty(Names))
            {
                value = filters.GetValueOrDefault(Names);
            }
            ViewContext.ViewBag.FilterValue = value;
        }

        var childContent = output.GetChildContentAsync().Result;

        if (Type == FilterType.BUTTON)
        {
            var button = new TagBuilder("button");
            foreach (var item in output.Attributes)
            {
                button.MergeAttribute(item.Name, item.Value?.ToString(), true);
            }

            button.MergeAttribute("type", "button", true);
            button.InnerHtml.AppendHtml(childContent.GetContent());
            filterContext?.ButtonFields.Add(button);

            output.SuppressOutput();
            return;
        }
        else if (Type == FilterType.HTML)
        {
            var button = new TagBuilder("div");
            foreach (var item in output.Attributes)
            {
                button.MergeAttribute(item.Name, item.Value?.ToString(), true);
            }

            button.InnerHtml.AppendHtml(childContent.GetContent());
            if (LgSize > 0)
            {
                button.AddCssClass(string.Format("col-lg-{0} col-md-{1}", lgSize, mdSize));
                filterContext?.HtmlFields.Add(new(button, this));
            }
            else
            {
                filterContext?.ButtonFields.Add(button);
            }

            output.SuppressOutput();
            return;
        }

        if (childContent != null && !childContent.IsEmptyOrWhiteSpace)
        {
            var html = childContent.GetContent();
            if (Type == FilterType.HIDDEN)
            {
                filterContext?.HiddenFields.Add(new HtmlString(html));
            }
            else
            {
                var div = new TagBuilder("div");
                div.AddCssClass(string.Format("col-lg-{0} col-md-{1}", lgSize, mdSize));
                div.InnerHtml.AppendHtml(html);
                filterContext?.Fields.Add(new(div, this));
            }
        }
        else
        {
            var div = new TagBuilder("div");
            div.AddCssClass(string.Format("col-lg-{0} col-md-{1}", lgSize, mdSize));
            if (filterContext != null && filterContext.ShowLabel)
            {
                div.AddCssClass("form-group");
                var label = new TagBuilder("label");
                label.AddCssClass("fw-bolder");
                label.InnerHtml.SetContent(Label ?? string.Empty);
                div.InnerHtml.AppendHtml(label);
            }
            if (Type == FilterType.TEXT || Type == FilterType.DATE || Type == FilterType.DECIMAL || Type == FilterType.INT || Type == FilterType.HIDDEN)
            {
                var input = new TagBuilder("input")
                {
                    TagRenderMode = TagRenderMode.SelfClosing
                };
                foreach (var item in output.Attributes)
                {
                    input.MergeAttribute(item.Name, item.Value?.ToString(), true);
                }
                if (Type != FilterType.HIDDEN)
                {
                    input.MergeAttribute("type", "text");
                    input.AddCssClass("form-control");
                    input.MergeAttribute("placeHolder", PlaceHolder, true);
                }
                else
                {
                    input.MergeAttribute("type", "hidden");
                }
                input.MergeAttribute("autocomplete", "off", true);
                input.MergeAttribute("data-field", Names, true);
                input.MergeAttribute("value", value, true);
                input.MergeAttribute("data-operant", operant, true);

                if (Type == FilterType.DATE)
                {
                    input.AddCssClass("date");
                }
                else if (Type == FilterType.DECIMAL)
                {
                    input.AddCssClass("float");
                }
                else if (Type == FilterType.INT)
                {
                    input.AddCssClass("integer");
                }

                if (Type != FilterType.HIDDEN)
                {
                    div.InnerHtml.AppendHtml(input);
                }
                else
                {
                    filterContext?.HiddenFields.Add(input);
                }
            }
            else if (Type == FilterType.SELECT)
            {
                var input = new TagBuilder("select");
                foreach (var item in output.Attributes)
                {
                    input.MergeAttribute(item.Name, item.Value?.ToString(), true);
                }
                input.MergeAttribute("data-field", Names, true);
                input.MergeAttribute("data-operant", operant, true);
                input.MergeAttribute("data-minimum-results-for-search", "Infinity", true);
                input.AddCssClass("select2");

                var emptyOption = new TagBuilder("option");
                emptyOption.MergeAttribute("value", "");
                emptyOption.InnerHtml.SetContent(PlaceHolder ?? string.Empty);
                input.InnerHtml.AppendHtml(emptyOption);

                if (Items != null)
                {
                    foreach (var item in Items)
                    {
                        var option = new TagBuilder("option");
                        option.MergeAttribute("value", item.Value);
                        option.InnerHtml.SetContent(item.Text);
                        if (item.Value == value)
                        {
                            option.MergeAttribute("selected", "selected");
                        }
                        input.InnerHtml.AppendHtml(option);
                    }
                }

                div.InnerHtml.AppendHtml(input);
            }
            else if (Type == FilterType.BOOL)
            {
                var input = new TagBuilder("select");
                foreach (var item in output.Attributes)
                {
                    input.MergeAttribute(item.Name, item.Value?.ToString(), true);
                }
                input.MergeAttribute("data-field", Names, true);
                input.MergeAttribute("data-operant", "=", true);
                input.MergeAttribute("data-minimum-results-for-search", "Infinity", true);
                input.AddCssClass("select2");

                var emptyOption = new TagBuilder("option");
                emptyOption.MergeAttribute("value", "");
                emptyOption.InnerHtml.SetContent(PlaceHolder ?? string.Empty);
                input.InnerHtml.AppendHtml(emptyOption);

                var items = new string[] { "true", "false" };
                foreach (var item in items)
                {
                    var option = new TagBuilder("option");
                    option.MergeAttribute("value", item);
                    option.InnerHtml.SetContent(item == "true" ? "Evet" : "Hayır");
                    if (item == value)
                    {
                        option.MergeAttribute("selected", "selected");
                    }
                    input.InnerHtml.AppendHtml(option);
                }

                div.InnerHtml.AppendHtml(input);
            }

            if (Type != FilterType.HIDDEN)
            {
                filterContext?.Fields.Add(new(div, this));
            }
        }

        output.SuppressOutput();
    }

    private string GetOperantString()
    {
        var operant = "*";
        switch (Operant)
        {
            case FilterOperant.CONTAINS:
                operant = "*";
                break;
            case FilterOperant.ENDS_WITH:
                operant = "-";
                break;
            case FilterOperant.EQUAL:
                operant = "=";
                break;
            case FilterOperant.GREATER_THAN:
                operant = ">";
                break;
            case FilterOperant.GREATER_THANEQUAL:
                operant = ">=";
                break;
            case FilterOperant.LESS_THAN:
                operant = "<";
                break;
            case FilterOperant.LESS_THANEQUAL:
                operant = "<=";
                break;
            case FilterOperant.NOT_CONTAINS:
                operant = "!*";
                break;
            case FilterOperant.NOT_EQUAL:
                operant = "!=";
                break;
            case FilterOperant.STARTS_WITH:
                operant = "+";
                break;

            default:
                break;
        }
        return operant;
    }
}

public class FilterContext
{
    public TagBuilder? HtmlContent { get; set; }
    public string? Id { get; set; }
    public List<(IHtmlContent Content, SearchFilterField Field)> Fields { get; set; }
    public List<IHtmlContent> HiddenFields { get; set; }
    public List<IHtmlContent> ButtonFields { get; set; }
    public List<(IHtmlContent Content, SearchFilterField Field)> HtmlFields { get; set; }
    public int ColumnSize { get; set; }
    public bool ShowLabel { get; set; }
    public bool Visible { get; set; }
    public FilterConfig? Config { get; set; }

    public FilterContext()
    {
        Fields = [];
        HiddenFields = [];
        ButtonFields = [];
        HtmlFields = [];
    }
}

public enum FilterButtonType
{
    ICON,
    BUTTON
}

public enum FilterType
{
    TEXT,
    INT,
    DECIMAL,
    SELECT,
    DATE,
    BOOL,
    HIDDEN,
    BUTTON,
    HTML
}
public enum FilterOperant
{
    CONTAINS,
    EQUAL,
    GREATER_THAN,
    GREATER_THANEQUAL,
    LESS_THAN,
    LESS_THANEQUAL,
    STARTS_WITH,
    ENDS_WITH,
    NOT_CONTAINS,
    NOT_EQUAL
}