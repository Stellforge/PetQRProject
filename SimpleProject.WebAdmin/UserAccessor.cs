using SimpleProject.Domain;
using SimpleProject.Domain.Dtos;
using SimpleProject.Services;
using System.Text;
using System.Text.Json;

namespace SimpleProject.WebAdmin;

public class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public UserAccessor(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment)
    {
        _httpContextAccessor = httpContextAccessor;
        _webHostEnvironment = webHostEnvironment;
    }
    public AdminUserDto? AdminUser { get => Get<AdminUserDto>("CurrentAdminUser"); set => Store("CurrentAdminUser", value); }
    public int? AdminUserId => AdminUser?.Id;
    public string? ClientIP
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Connection != null)
            {
                return httpContext.Connection.RemoteIpAddress?.ToString();
            }
            return null;
        }
    }
    public string? RequestLink
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Request != null)
            {
                return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
            }
            return null;
        }
    }
    public string WebRootPath => _webHostEnvironment.WebRootPath;
    public IServiceProvider RequestServiceProvider
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.RequestServices != null)
            {
                return httpContext.RequestServices;
            }
            else
            {
                throw new ArgumentException("HttpContext");
            }
        }
    }
    public int TimeZoneOffset
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Request != null)
            {
                var cookieValue = httpContext.Request.Cookies["_tzo"];
                if (cookieValue != null && int.TryParse(cookieValue, out int offset))
                {
                    return offset;
                }
            }
            return Convert.ToInt32(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours);
        }
    }

    public void Clear(string? key = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Session != null)
        {
            if (string.IsNullOrEmpty(key))
            {
                httpContext.Session.Clear();
            }
            else if (httpContext.Session.Keys.Contains(key))
            {
                httpContext.Session.Remove(key);
            }
        }
        else
        {
            throw new ArgumentException("HttpContext");
        }
    }
    public T? Get<T>(string key)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Session != null)
        {
            var value = httpContext.Session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value, JsonOptions.Default);
        }
        else
        {
            throw new ArgumentException("HttpContext");
        }
    }
    public void Store<T>(string key, T data)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Session != null)
        {
            httpContext.Session.SetString(key, JsonSerializer.Serialize(data, JsonOptions.Default));
        }
        else
        {
            throw new ArgumentException("HttpContext");
        }
    }

    public string? GetLocal(string key)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            if (httpContext.Request != null && httpContext.Request.Cookies != null)
            {
                return httpContext.Request.Cookies[key];
            }
        }
        else
        {
            throw new ArgumentException("HttpContext");
        }
        return default;
    }
    public void StoreLocal(string key, string value, int expires = 30)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Response?.Cookies.Append(key, value, new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(expires)
            });
        }
        else
        {
            throw new ArgumentException("HttpContext");
        }
    }
}