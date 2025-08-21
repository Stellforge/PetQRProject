using SimpleProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace SimpleProject.WebAdmin;

public class SessionFilterAttribute : TypeFilterAttribute
{
    public SessionFilterAttribute() : base(typeof(SessionFilter))
    {
    }
    private class SessionFilter : IAuthorizationFilter
    {
        public SessionFilter()
        {
        }

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            ArgumentNullException.ThrowIfNull(filterContext);

            var allowAnonym = filterContext.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            var returnUrl = filterContext.HttpContext.Request.Path + filterContext.HttpContext.Request.QueryString;
            if (returnUrl != "/")
            {
                returnUrl = "?returnUrl=" + HttpUtility.UrlEncode(returnUrl);
            }

            if (!allowAnonym)
            {
                var userAccessor = filterContext.HttpContext.RequestServices.GetRequiredService<IUserAccessor>();
                if (!userAccessor.AdminUserId.HasValue)
                {
                    if (IsAjax(filterContext.HttpContext.Request))
                    {
                        filterContext.Result = new JsonResult(new
                        {
                            HasError = true,
                            Redirect = "/home/login"
                        });
                        return;
                    }
                    filterContext.Result = new RedirectResult("/home/login" + returnUrl);
                    return;
                }
            }
        }

        private static bool IsAjax(HttpRequest req)
        {
            return req != null && req.Headers != null && req.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
