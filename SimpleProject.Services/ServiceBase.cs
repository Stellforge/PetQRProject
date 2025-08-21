using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleProject.Services;

public interface IServiceBase
{
    Task<Result<T>> Get<T>(Query<T>? query) where T : Entity, new ();
    Task<Result<T>> Get<T>(Expression<Func<T, bool>>? filter, params string[] includes) where T : Entity, new();
    Task<Result<T>> Get<T>(Expression<Func<T, bool>>? filter, Expression<Func<T, T>>? select, bool addSelect = false) where T : Entity, new();

    Task<Result<IEnumerable<T>>> Query<T>(Expression<Func<T, bool>>? filter, params string[] includes) where T : Entity, new();
    Task<Result<IEnumerable<T>>> Query<T>(Expression<Func<T, bool>>? filter, Expression<Func<T, T>>? select, bool addSelect = false) where T : Entity, new();
    Task<Result<IEnumerable<T>>> Query<T>(Query<T>? query) where T : Entity, new();
    Task<Result<(IEnumerable<T> Data, int Total)?>> QueryWithTotal<T>(Query<T> query) where T : Entity, new();

    Task<Result<bool>> Any<T>(Expression<Func<T, bool>>? filter) where T : Entity, new();

    Task<Result> ExecuteTransaction(Func<Task<Result>> func);

    Expression<Func<T, bool>> NewQuery<T>(Expression<Func<T, bool>> filter);
    Expression<Func<T, T>> SelectAllFields<T>(Expression<Func<T, T>>? extra = null);
    string ReplaceAllFieldValues<T>(string input, T data);

    ExcelColumn<T> GetEnumExcelColum<T, TProp>(Expression<Func<T, TProp>> field) where T : new() where TProp : Enum;
    ExcelColumn<T> GetExcelColum<T>(string title, string name, Expression<Func<T, object?>> get, Expression<Action<T, object?>> set) where T : new();
}

public class ServiceBase : IServiceBase
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly ILogService _logService;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IValidationService _validation;
    protected readonly IUserAccessor _userAccessor;

    public ServiceBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logService = _serviceProvider.GetRequiredService<ILogService>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        _validation = _serviceProvider.GetRequiredService<IValidationService>();
        _userAccessor = _serviceProvider.GetRequiredService<IUserAccessor>();
    }

    public async Task<Result<T>> Get<T>(Query<T>? query) where T : Entity, new()
    {
        try
        {
            var repository = _serviceProvider.GetRequiredService<IRepository<T>>();
            var data = await repository.Get(query);
            return new Result<T>() { Data = data };
        }
        catch (Exception ex)
        {
            return new Result<T>(await _logService.LogException(ex));
        }
    }
    public async Task<Result<T>> Get<T>(Expression<Func<T, bool>>? filter, params string[] includes) where T : Entity, new()
    {
        try
        {
            var repository = _serviceProvider.GetRequiredService<IRepository<T>>();
            var data = await repository.Get(filter, includes);
            return new Result<T>() { Data = data };
        }
        catch (Exception ex)
        {
            return new Result<T>(await _logService.LogException(ex));
        }
    }
    public async Task<Result<T>> Get<T>(Expression<Func<T, bool>>? filter, Expression<Func<T, T>>? select = null, bool addSelect = false) where T : Entity, new()
    {
        try
        {
            var repository = _serviceProvider.GetRequiredService<IRepository<T>>();
            if (!addSelect)
            {
                var data = await repository.Get(filter, select);
                return new Result<T>() { Data = data };
            }
            else
            {
                var data = await repository.Get(filter, SelectAllFields(select));
                return new Result<T>() { Data = data };
            }
        }
        catch (Exception ex)
        {
            return new Result<T>(await _logService.LogException(ex));
        }
    }

    public async Task<Result<IEnumerable<T>>> Query<T>(Expression<Func<T, bool>>? filter, params string[] includes) where T : Entity, new()
    {
        try
        {
            var repository = _serviceProvider.GetRequiredService<IRepository<T>>();
            var data = await repository.Query(filter, includes);
            return new Result<IEnumerable<T>>() { Data = data };
        }
        catch (Exception ex)
        {
            return new Result<IEnumerable<T>>(await _logService.LogException(ex));
        }
    }
    public async Task<Result<IEnumerable<T>>> Query<T>(Expression<Func<T, bool>>? filter, Expression<Func<T, T>>? select = null, bool addSelect = false) where T : Entity, new()
    {
        try
        {
            var repository = _serviceProvider.GetRequiredService<IRepository<T>>();
            if (!addSelect)
            {
                var data = await repository.Query(filter, select);
                return new Result<IEnumerable<T>>() { Data = data };
            }
            else
            {
                var data = await repository.Query(filter, SelectAllFields(select));
                return new Result<IEnumerable<T>>() { Data = data };
            }
        }
        catch (Exception ex)
        {
            return new Result<IEnumerable<T>>(await _logService.LogException(ex));
        }
    }
    public async Task<Result<IEnumerable<T>>> Query<T>(Query<T>? query) where T : Entity, new()
    {
        try
        {
            var repository = _serviceProvider.GetRequiredService<IRepository<T>>();
            var data = await repository.Query(query);
            return new Result<IEnumerable<T>>() { Data = data };
        }
        catch (Exception ex)
        {
            return new Result<IEnumerable<T>>(await _logService.LogException(ex));
        }
    }
    public async Task<Result<(IEnumerable<T> Data, int Total)?>> QueryWithTotal<T>(Query<T> query) where T : Entity, new()
    {
        try
        {
            var repository = _serviceProvider.GetRequiredService<IRepository<T>>();
            var data = await repository.QueryWithTotal(query);
            return new Result<(IEnumerable<T> Data, int Total)?>() { Data = data };
        }
        catch (Exception ex)
        {
            return new Result<(IEnumerable<T> Data, int Total)?>(await _logService.LogException(ex));
        }
    }

    public async Task<Result<bool>> Any<T>(Expression<Func<T, bool>>? filter) where T : Entity, new()
    {
        try
        {
            var repository = _serviceProvider.GetRequiredService<IRepository<T>>();
            var data = await repository.Any(filter);
            return new Result<bool>() { Data = data };
        }
        catch (Exception ex)
        {
            return new Result<bool>(await _logService.LogException(ex));
        }
    }

    public async Task<Result> ExecuteTransaction(Func<Task<Result>> func)
    {
        try
        {
            await _unitOfWork.BeginTransaction();

            var result = await func.Invoke();
            if (result.HasError)
            {
                await _unitOfWork.RollbackTransaction();
            }
            else
            {
                await _unitOfWork.CommitTransaction();
                await _logService.WriteEntityHistories();
            }

            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransaction();
            return new Result(await _logService.LogException(ex));
        }
    }

    public Expression<Func<T, bool>> NewQuery<T>(Expression<Func<T, bool>> filter)
    {
        return filter;
    }
    public Expression<Func<T, T>> SelectAllFields<T>(Expression<Func<T, T>>? extra = null)
    {
        var arg = Expression.Parameter(typeof(T), "a");
        var newExpression = Expression.New(typeof(T));

        var bindings = new List<MemberBinding>();
        if (extra != null && extra.Body is MemberInitExpression)
        {
            arg = extra.Parameters.First();
        }
        foreach (var item in typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance))
        {
            if (!item.PropertyType.IsValueType())
            {
                continue;
            }
            bindings.Add(Expression.Bind(item, Expression.Property(arg, item.Name)));
        }
        if (extra != null && extra.Body is MemberInitExpression)
        {
            if (extra.Body is MemberInitExpression additionalInit)
            {
                foreach (var additionalBinding in additionalInit.Bindings)
                {
                    var extBinding = bindings.FirstOrDefault(a => a.Member == additionalBinding.Member);
                    if (extBinding != null)
                    {
                        bindings.Remove(extBinding);
                    }
                    bindings.Add(additionalBinding);
                }
            }
        }

        var memberInitExpression = Expression.MemberInit(newExpression, bindings);
        return Expression.Lambda<Func<T, T>>(memberInitExpression, arg);
    }
    public string ReplaceAllFieldValues<T>(string input, T data)
    {
        var result = input;
        foreach (var item in typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance))
        {
            if (!item.PropertyType.IsValueType())
            {
                continue;
            }
            result = result.Replace("[" + item.Name + "]", item.GetValue(data)?.ToString() ?? "");
        }
        return result;
    }

    public ExcelColumn<T> GetEnumExcelColum<T, TProp>(Expression<Func<T, TProp>> field) where T : new() where TProp : Enum
    {
        var memberExpression = field.Body.FindMemberExpression() ?? throw new ArgumentException("memberExpression");
        var name = memberExpression.ToString().TrimStart((field.Parameters.First().ToString() + ".").ToCharArray());

        var property = memberExpression.Member as PropertyInfo ?? throw new ArgumentException("property");
        var title = property.Name;

        var displayName = property.GetCustomAttribute(typeof(DisplayAttribute), true);
        if (displayName != null)
        {
            title = ((DisplayAttribute)displayName).GetName();
        }

        var fieldGetter = property.GetPropGetter<T>();
        var fieldSetter = property.GetPropSetter<T>();
        var hasFlag = typeof(TProp).GetCustomAttributes<FlagsAttribute>().Any();

        object? select(T data)
        {
            var display = "";
            var value = fieldGetter != null ? fieldGetter(data) : null;
            if (value != null)
            {
                if (hasFlag)
                {
                    var enumValue = (TProp)value;
                    var items = new List<string?>();
                    foreach (TProp item in Enum.GetValues(typeof(TProp)))
                    {
                        if (Convert.ToInt32(item) == 0)
                        {
                            continue;
                        }
                        if (enumValue.HasFlag(item))
                        {
                            items.Add(item.GetDisplayName());
                        }
                    }
                    display = string.Join(",", items);
                }
                else
                {
                    if (value is TProp prop)
                    {
                        display = prop.GetDisplayName();
                    }
                }
                if (string.IsNullOrEmpty(display))
                {
                    throw new InvalidCastException(title);
                }
            }
            return display;
        }

        void set(T data, object? value)
        {
            if (fieldSetter != null)
            {
                var found = false;
                var display = Convert.ToString(value);
                if (hasFlag && value != null)
                {
                    var displays = display?.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();
                    if (displays != null)
                    {
                        var enumValue = Enum.Parse(typeof(TProp), "0");
                        foreach (var item in displays)
                        {
                            foreach (TProp itemValue in Enum.GetValues(typeof(TProp)))
                            {
                                if (string.Equals(itemValue.GetDisplayName(), item, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    enumValue = (int)enumValue | Convert.ToInt32(itemValue);
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (found)
                        {
                            fieldSetter.Invoke(data, enumValue);
                        }
                    }
                }
                else if (value != null)
                {
                    foreach (TProp item in Enum.GetValues(typeof(TProp)))
                    {
                        if (string.Equals(item.GetDisplayName(), display, StringComparison.InvariantCultureIgnoreCase))
                        {
                            fieldSetter.Invoke(data, item);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    throw new InvalidCastException(title);
                }
            }
            else
            {
                throw new InvalidCastException(title);
            }
        }

        return new ExcelColumn<T>()
        {
            DataType = ExcelDataType.STRING,
            Name = name,
            Title = title,
            PropertyType = typeof(string),
            Select = select,
            Set = set
        };
    }
    public ExcelColumn<T> GetExcelColum<T>(string title, string name, Expression<Func<T, object?>> get, Expression<Action<T, object?>> set) where T : new()
    {
        return new ExcelColumn<T>()
        {
            DataType = ExcelDataType.STRING,
            Name = name,
            Title = title,
            PropertyType = typeof(string),
            Select = get.Compile(),
            Set = set.Compile()
        };
    }
}