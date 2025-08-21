using SimpleProject.Domain.Dtos.Admin;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain;
using SimpleProject.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SimpleProject.WebAdmin;

public static class ViewExtensions
{
    public static T? GetCookie<T>(this RazorPage page, string key)
    {
        var httpRequest = page.ViewContext.HttpContext.Request;
        if (httpRequest != null && httpRequest.Cookies != null)
        {
            if (httpRequest.Cookies.TryGetValue(key, out string? value))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch { }
                }
            }
        }
        return default;
    }
    public static string GetErrorMessage(this RazorPage page, string seperator = "<br />")
    {
        return string.Join(seperator, page.ViewContext.ModelState.GetErrorMessages());
    }
    public static DateTime ToUserLocalDate(this DateTime date, RazorPage page)
    {
        var userAccessor = page.ViewContext.HttpContext.RequestServices.GetService<IUserAccessor>();
        if (userAccessor != null)
        {
            return DateTime.SpecifyKind(date.ToUniversalTime().AddMinutes(userAccessor.TimeZoneOffset), DateTimeKind.Unspecified);
        }
        return date.ToLocalTime();
    }
    public static DateTime ToUserUtcDate(this DateTime date, RazorPage page)
    {
        var userAccessor = page.ViewContext.HttpContext.RequestServices.GetService<IUserAccessor>();
        if (userAccessor != null && date.Kind != DateTimeKind.Utc)
        {
            return DateTime.SpecifyKind(date.AddMinutes(-userAccessor.TimeZoneOffset), DateTimeKind.Utc);
        }
        return date.ToUniversalTime();
    }

    public static List<SelectListItem> GetFlagsSelectList<T>(this RazorPage page, T? value) where T : Enum
    {
        if (page is null)
        {
            throw new ArgumentNullException(nameof(page));
        }

        var type = typeof(T);
        var list = new List<SelectListItem>();
        foreach (var item in Enum.GetValues(type))
        {
            if (((int)item) == 0)
            {
                continue;
            }
            list.Add(new SelectListItem()
            {
                Value = ((int)item).ToString(),
                Text = ((T)item).GetDisplayName(),
                Selected = value != null && value.HasFlag((T)item),
            });
        }
        return list;
    }
    public static string EnumDisplayNameJson<T>(this RazorPage page) where T : Enum
    {
        if (page is null)
        {
            throw new ArgumentNullException(nameof(page));
        }

        var items = new List<string>();
        var type = typeof(T);
        foreach (Enum item in Enum.GetValues(type))
        {
            var displayName = item.GetDisplayName();
            items.Add("{" + string.Format("\"name\":\"{0}\", \"value\":{1}", (string.IsNullOrEmpty(displayName) ? item.ToString() : displayName).Replace("\"", "\\\""), Convert.ToInt32(item)) + "}");
        }
        return "[" + string.Join(", ", items) + "]";
    }
    public static string EnumValueJson<T>(this RazorPage page) where T : Enum
    {
        if (page is null)
        {
            throw new ArgumentNullException(nameof(page));
        }

        var items = new List<string>();
        var type = typeof(T);
        foreach (Enum item in Enum.GetValues(type))
        {
            items.Add(string.Format("\"{0}\":{1}", item, Convert.ToInt32(item)));
        }
        return "{" + string.Join(", ", items) + "}";
    }

    public static AdminUserDto? GetUser(this RazorPage page)
    {
        var httpContext = page.ViewContext.HttpContext;
        if (httpContext != null)
        {
            var userAccessor = httpContext.RequestServices.GetService<IUserAccessor>();
            if (userAccessor != null)
            {
                return userAccessor.AdminUser;
            }
        }
        return default;
    }
    public static List<MenuItem> GetMenu(this RazorPage page)
    {
        var httpContext = page.ViewContext.HttpContext;
        if (httpContext != null)
        {
            var userAccessor = httpContext.RequestServices.GetService<IUserAccessor>();
            if (userAccessor != null)
            {
                return userAccessor.Get<List<MenuItem>>("LeftMenu") ?? new List<MenuItem>();
            }
        }
        return new List<MenuItem>();
    }
}
