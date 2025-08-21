using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;
using System.Reflection;

namespace SimpleProject.WebAdmin;

public class DateConvertFilterAttribute : ActionFilterAttribute
{
    private static ConcurrentDictionary<Type, List<PropertyInfo>> _DateProperties = new();

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Controller is Controller controller)
        {
            if (context.Result is ViewResult viewResult)
            {
                UpdateLocalUserDate(viewResult.Model, controller);
            }
            else if (context.Result is JsonResult jsonResult)
            {
                UpdateLocalUserDate(jsonResult.Value, controller);
            }
        }
        base.OnActionExecuted(context);
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is Controller controller)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null)
                {
                    continue;
                }

                UpdateUtcUserDate(argument, controller);
            }
        }
        base.OnActionExecuting(context);
    }

    private static void UpdateLocalUserDate(object? model, Controller controller)
    {
        if (model == null)
        {
            return;
        }

        if (!_DateProperties.TryGetValue(model.GetType(), out var dateProperties))
        {
            dateProperties = [.. model.GetType().GetProperties().Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))];
            _DateProperties[model.GetType()] = dateProperties;
        }

        foreach (var property in dateProperties)
        {
            var value = property.GetValue(model) as DateTime?;
            if (value.HasValue)
            {
                property.SetValue(model, value.Value.ToUserLocalDate(controller));
            }
        }
    }
    private static void UpdateUtcUserDate(object? model, Controller controller)
    {
        if (model == null)
        {
            return;
        }

        if (!_DateProperties.TryGetValue(model.GetType(), out var dateProperties))
        {
            dateProperties = [.. model.GetType().GetProperties().Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))];
            _DateProperties[model.GetType()] = dateProperties;
        }

        foreach (var property in dateProperties)
        {
            var value = property.GetValue(model) as DateTime?;
            if (value.HasValue)
            {
                property.SetValue(model, value.Value.ToUserUtcDate(controller));
            }
        }
    }
}
