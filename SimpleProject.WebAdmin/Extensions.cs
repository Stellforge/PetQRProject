using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain;
using SimpleProject.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleProject.Domain.Dtos.Admin;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace SimpleProject.WebAdmin;

public static class Extensions
{
    public static IActionResult ErrorJson(this Controller controller, string errorMsg)
    {
        return controller is null ? throw new ArgumentNullException(nameof(controller)) : new Result(errorMsg).ToJson();
    }
    public static IActionResult ErrorJson(this Controller controller, ModelStateDictionary modelState)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var errors = new List<string?>();
        foreach (var error in modelState.Values.Where(a => a.Errors.Count > 0))
        {
            foreach (var item in error.Errors)
            {
                errors.Add(item.ErrorMessage);
            }
        }
        return new Result() { Errors = errors }.ToJson();
    }
    public static ActionResult SuccesJson(this Controller controller, object? data = null)
    {
        ArgumentNullException.ThrowIfNull(controller);

        return new JsonResult(new { HasError = false, Data = data });
    }
    public static ActionResult RedirectJson(this Controller controller, string url)
    {
        ArgumentNullException.ThrowIfNull(controller);

        return new JsonResult(new { HasError = false, Redirect = url });
    }
    public static IActionResult ToJson(this Result result)
    {
        return new JsonResult(result);
    }

    public static IActionResult ToView<T>(this Result<T> result, Controller controller, string? viewPath = null)
    {
        return result.ToView(controller, result.Data, viewPath);
    }
    public static IActionResult ToView(this Result result, Controller controller, object? data = null, string? viewPath = null)
    {
        if (data != null)
        {
            controller.ViewData.Model = data;
        }
        if (result.Errors != null && result.Errors.Count != 0)
        {
            controller.ModelState.Clear();
            result.Errors.ForEach(a => controller.ModelState.AddModelError("", a ?? string.Empty));
        }
        return new ViewResult()
        {
            ViewName = viewPath,
            ViewData = controller.ViewData,
            TempData = controller.TempData,
        };
    }
    public static IActionResult ToPartialView<T>(this Result<T> result, Controller controller, string? viewPath = null)
    {
        return result.ToPartialView(controller, result.Data, viewPath);
    }
    public static IActionResult ToPartialView(this Result result, Controller controller, object? data = null, string? viewPath = null)
    {
        if (data != null)
        {
            controller.ViewData.Model = data;
        }
        if (result.Errors != null && result.Errors.Count != 0)
        {
            controller.ModelState.Clear();
            result.Errors.ForEach(a => controller.ModelState.AddModelError("", a ?? string.Empty));
        }
        return new PartialViewResult()
        {
            ViewName = viewPath,
            ViewData = controller.ViewData,
            TempData = controller.TempData
        };
    }
    public static IActionResult Excel<T>(this Result<IEnumerable<T>> result, Controller controller, string fileName = "data") where T : new()
    {
        if (result.HasError)
        {
            return TextFile(controller, result);
        }
        if (result.Data == null)
        {
            return TextFile(controller, new Exception(string.Format("Excel datası bulunamadı")));
        }
        var columns = result.Value<List<ExcelColumn<T>>>("Columns");
        if (columns == null)
        {
            return TextFile(controller, new Exception(string.Format("Excel kolonları bulanamadı")));
        }

        using var stream = new MemoryStream();
        var excelService = controller.HttpContext.RequestServices.GetRequiredService<IExcelService>();
        excelService.Save(stream, result.Data, columns);
        return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = fileName + ".xlsx"
        };
    }
    public static IActionResult TextFile(this Controller controller, Result result)
    {
        return controller.File(Encoding.UTF8.GetBytes(string.Join(",", result.Errors)), "text/plain", "error.txt");
    }
    public static IActionResult TextFile(this Controller controller, Exception ex)
    {
        return controller.File(Encoding.UTF8.GetBytes(string.Join(",", ex.Message)), "text/plain", "error.txt");
    }

    public static async Task<IActionResult> UploadExcel<T>(this Controller controller, IFormFile file, Func<ExcelUploadDto, Task<Result>> saveMethod) where T : Entity
    {
        var webHostEnvironment = controller.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var fileService = controller.HttpContext.RequestServices.GetRequiredService<IFileService>();
        var excelService = controller.HttpContext.RequestServices.GetRequiredService<IExcelService>();
        var userAccessor = controller.HttpContext.RequestServices.GetRequiredService<IUserAccessor>();

        if (file == null || file.Length == 0)
        {
            return controller.ErrorJson("Dosya bulunamadı");
        }

        var filePath = Path.Combine(webHostEnvironment.WebRootPath, "upload", "excel", userAccessor.AdminUserId!.Value.ToString(), Guid.NewGuid().ToString() + ".xlsx");
        var fileResult = await fileService.SaveFile(file.OpenReadStream(), filePath);
        if (fileResult.HasError)
        {
            return fileResult.ToJson();
        }

        var entityName = typeof(T).Name;
        var uploadData = new ExcelUploadDto()
        {
            AdminUserId = userAccessor.AdminUserId!.Value,
            FilePath = filePath,
            InterfaceName = "I" + entityName + "Service",
            Method = "Save" + entityName + "Excel",
            Name = entityName,
            UploadType = entityName
        };

        var rowCountResult = excelService.GetRowCount(filePath);
        if (rowCountResult.HasError || rowCountResult.Data == 0)
        {
            var errorMessage = "Kayıt bulunamaıd";
            if (rowCountResult.HasError)
            {
                errorMessage = rowCountResult.GetErrorMessage();
            }

            await fileService.DeleteFile(filePath);
            return controller.ErrorJson(errorMessage);
        }
        if (AppSettings.Current.MaxExcelRowCount == 0 || AppSettings.Current.MaxExcelRowCount >= rowCountResult.Data)
        {
            var excelResult = await saveMethod(uploadData);
            if (excelResult.HasError)
            {
                return excelResult.ToJson();
            }
            if (!string.IsNullOrEmpty(uploadData.ErrorFilePath))
            {
                return controller.SuccesJson(new { Started = true, FilePath = uploadData.ErrorFilePath });
            }
            return controller.SuccesJson(new { Started = true });
        }

        var result = await excelService.SaveExcelUpload(uploadData);
        if (result.HasError)
        {
            return result.ToJson();
        }

        return controller.SuccesJson(new { Started = false });
    }

    public static async Task<string> RenderPartialViewAsync<TModel>(this Controller controller, string viewNamePath, TModel model)
    {
        if (string.IsNullOrEmpty(viewNamePath))
        {
            viewNamePath = controller.ControllerContext.ActionDescriptor.ActionName;
        }
        controller.ViewData.Model = model;

        using var writer = new StringWriter();
        try
        {
            IViewEngine viewEngine = controller.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();
            ViewEngineResult? viewResult = null;
            if (viewNamePath.EndsWith(".cshtml"))
            {
                viewResult = viewEngine.GetView(viewNamePath, viewNamePath, false);
            }
            else
            {
                viewResult = viewEngine.FindView(controller.ControllerContext, viewNamePath, false);
            }

            if (!viewResult.Success)
            {
                return $"A view with the name '{viewNamePath}' could not be found";
            }

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);

            return writer.GetStringBuilder().ToString();
        }
        catch (Exception ex)
        {
            return $"Failed - {ex.Message}";
        }
    }

    public static void AddModelError(this Result result, Controller controller)
    {
        if (result.Errors != null)
        {
            foreach (var item in result.Errors.Where(a => !string.IsNullOrEmpty(a)))
            {
                controller.ModelState.AddModelError("", item!);
            }
        }
    }
    public static IEnumerable<string> GetErrorMessages(this ModelStateDictionary modelState)
    {
        return modelState.Values.Where(a => a.Errors.Count > 0).SelectMany(a => a.Errors.Select(b => WebUtility.HtmlEncode(b.ErrorMessage)));
    }
    public static DateTime ToUserLocalDate(this DateTime date, Controller controller)
    {
        var userAccessor = controller.HttpContext.RequestServices.GetService<IUserAccessor>();
        if (userAccessor != null)
        {
            return DateTime.SpecifyKind(date.ToUniversalTime().AddMinutes(userAccessor.TimeZoneOffset), DateTimeKind.Unspecified);
        }
        return date.ToLocalTime();
    }
    public static DateTime ToUserUtcDate(this DateTime date, Controller controller)
    {
        var userAccessor = controller.HttpContext.RequestServices.GetService<IUserAccessor>();
        if (userAccessor != null && date.Kind != DateTimeKind.Utc)
        {
            return DateTime.SpecifyKind(date.AddMinutes(-userAccessor.TimeZoneOffset), DateTimeKind.Utc);
        }
        return date.ToUniversalTime();
    }
    public static string? GetRequestParam(this HttpRequest request, string key)
    {
        if (request.Query.TryGetValue(key, out StringValues queryValue))
        {
            return queryValue.ToString();
        }
        else if (request.Form.TryGetValue(key, out StringValues formValue))
        {
            return formValue.ToString();
        }
        return null;
    }

    public static Query<T> GetSelectQuery<T>(this Controller controller, GridRequest request, Action<Query<T>, string> filter, params string[]? exceptions) where T : Entity
    {
        var httpRequest = controller.HttpContext.Request;
        var gridRequest = controller.AddFilters<T>(request, exceptions);
        var query = request.GetQuery<T>();

        var q = httpRequest.GetRequestParam("q");
        var page = httpRequest.GetRequestParam("page");
        var id = httpRequest.GetRequestParam("id");

        if (!string.IsNullOrEmpty(q))
        {
            filter?.Invoke(query, q);
        }
        if (!string.IsNullOrEmpty(page) && int.TryParse(page, out int intPage))
        {
            intPage = intPage <= 0 ? 1 : intPage;
            query.Top = 20;
            query.Skip = (intPage - 1) * 20;
        }

        if (!string.IsNullOrEmpty(id))
        {
            var ids = new List<int>();
            if (id.Contains(','))
            {
                foreach (var item in id.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    try
                    {
                        ids.Add(Convert.ToInt32(item));
                    }
                    catch { }
                }
                query.Filters!.Add(a => ids.Contains(a.Id));
            }
            else
            {
                query.Filters!.Add(a => a.Id == Convert.ToInt32(id));
            }
        }
        return query;
    }

    public static IActionResult GetSelectResult<T>(this Result<(IEnumerable<T> Data, int Total)?> result, Query<T> query, Func<T, ListItem> getSelectData, bool? nullable = null, Action<List<ListItem>>? process = null)
    {
        if (result.HasError)
        {
            _ = new ContentResult
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Content = result.GetErrorMessage(),
                ContentType = "text/plain",
            };
        }

        var results = result.Data?.Data.Select(a => getSelectData(a))?.ToList();
        if (nullable.GetValueOrDefault() && query.Skip == 0)
        {
            results?.Insert(0, new ListItem()
            {
                Id = "",
                Text = "--Seçiniz--"
            });
        }

        if (process != null)
        {
            results ??= [];
            process.Invoke(results);
        }

        return new JsonResult(new
        {
            Results = results,
            Pagination = new
            {
                More = result.Data?.Total > query.Top + query.Skip,
            }
        });
    }
    public static IActionResult GetGridResult<T>(this Result<(IEnumerable<T> Data, int Total)?> result, GridRequest request, Dictionary<string, Func<T, Controller, object?>> columns, Controller controller)
    {
        if (result.HasError)
        {
            return result.ToJson();
        }

        var globalType = typeof(T);
        var fieldValueFns = new Dictionary<string, Delegate>();
        var arg = Expression.Parameter(globalType, "a");
        foreach (var field in request.Fields ?? [])
        {
            if (field.Contains('.'))
            {
                var type = globalType;
                var properties = field.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
                Expression expression = arg;
                var valid = true;
                
                var i = 0;
                var expressions = new List<Expression>();
                foreach (var property in properties)
                {
                    var pInfo = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (pInfo == null)
                    {
                        valid = false;
                        break;
                    }

                    expression = Expression.Property(expression, pInfo);
                    if (i < properties.Length - 1 || pInfo.PropertyType.IsNullableType())
                    {
                        expressions.Add(Expression.NotEqual(expression, Expression.Constant(null)));
                    }
                    type = pInfo.PropertyType;
                    i++;
                }
                
                if (valid)
                {
                    var currentExpression = expressions.First();
                    if (expressions.Count > 1)
                    {
                        currentExpression = Expression.AndAlso(expressions.First(), expressions.Skip(1).First());
                        foreach (var testExpression in expressions.Skip(2))
                        {
                            currentExpression = Expression.AndAlso(currentExpression, testExpression);
                        }
                    }
                    expression = Expression.Condition(currentExpression, Expression.Convert(expression, typeof(object)), Expression.Constant(null));

                    fieldValueFns[field] = Expression.Lambda(expression, arg).Compile();  
                }
            }
            else
            {
                var propInfo = globalType.GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propInfo != null)
                {
                    fieldValueFns[field] = Expression.Lambda(Expression.Property(arg, propInfo), arg).Compile();                
                }
            }
        }
        
        return new JsonResult(new
        {
            Data = result.Data?.Data.Select(a =>
            {
                var data = new object?[request.Fields?.Count ?? 0];
                for (int i = 0; i < (request.Fields?.Count ?? 0); i++)
                {
                    var field = request.Fields![i];
                    if (columns != null && columns.TryGetValue(field, out var fn))
                    {
                        data[i] = fn.Invoke(a, controller);
                    }
                    else if(fieldValueFns.TryGetValue(field, out var valueFn))
                    {
                        data[i] = valueFn.DynamicInvoke(a);
                    }
                    else
                    {
                        data[i] = null;
                    }
                }
                return data;
            }).ToList(),
            result.Data?.Total
        });
    }
    public static GridRequest AddFilters<T>(this Controller controller, GridRequest? request, params string[]? exceptions)
    {
        request ??= new GridRequest();
        request.Filters ??= [];

        var httpRequest = controller.HttpContext.Request;
        var propertyNames = typeof(T).GetProperties().Where(a => a.Name != "Id").Select(a => a.Name).ToList();
        foreach (var item in httpRequest.Query)
        {
            if (propertyNames.Any(a => string.Equals(a, item.Key, StringComparison.OrdinalIgnoreCase)) && (exceptions == null || !exceptions.Contains(item.Key, StringComparer.OrdinalIgnoreCase)))
            {
                if (string.IsNullOrEmpty(item.Value.ToString()))
                {
                    continue;
                }
                request.Filters.Add(new GridFilter()
                {
                    Field = item.Key,
                    Operant = "=",
                    Value = item.Value.ToString()
                });
            }
        }
        return request;
    }

    public static void StoreRequest(this Controller controller, GridRequest? request)
    {
        if (request == null || string.IsNullOrEmpty(request.SessionKey))
        {
            return;
        }
        var userAccessor = controller.HttpContext.RequestServices.GetRequiredService<IUserAccessor>();
        userAccessor.Store(request.SessionKey!, request);
    }
}


