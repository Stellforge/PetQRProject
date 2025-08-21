using SimpleProject.Data;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Nodes;

namespace SimpleProject.Services;

public interface ILogService : IScopedService
{
    Task<string> LogException(Exception ex);
    Task LogEntityHistory<T>(T data, T? old) where T : Entity;
    Task LogDeleteHistory<T>(T data) where T : Entity;
    Task WriteEntityHistories();
    Task ClearEntityHistories();
}

public class LogService : ILogService
{
    private readonly LogDbContext _dbContext;
    private readonly UnitOfWork _unitOfWork;
    private readonly Repository<ErrorLog> _repositoryErrorLog;
    private readonly Repository<EntityLog> _repositoryEntityLog;
    private readonly IUserAccessor _userAccessor;
    private readonly IUnitOfWork _globalUnitOfWork;

    public LogService(LogDbContext dbContext, IUserAccessor userAccessor, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _userAccessor = userAccessor;
        _unitOfWork = new(_dbContext);
        _repositoryErrorLog = new(_dbContext, _unitOfWork);
        _repositoryEntityLog = new(_dbContext, _unitOfWork);
        _globalUnitOfWork = unitOfWork;
    }

    private readonly List<EntityLog> _EntityLogs = [];

    public async Task<string> LogException(Exception ex)
    {
        try
        {
            if (ex is not BusException)
            {
                var log = new ErrorLog()
                {
                    ClientIP = _userAccessor.ClientIP,
                    ErrorMessage = GetErrorMessage(ex, 0),
                    RequestLink = _userAccessor.RequestLink,
                    AdminUserId = _userAccessor.AdminUserId
                };

                await _repositoryErrorLog.Add(log);
            }
        }
        catch //(Exception e)
        {
        }
        return GetErrorString(ex);
    }
    public async Task LogEntityHistory<T>(T data, T? old) where T : Entity
    {
        try
        {
            
            var log = GetEntityLog(data, old);
            if (log != null)
            {
                if (_globalUnitOfWork.IsTransactional())
                {
                    _EntityLogs.Add(log);
                }
                else
                {
                    await _repositoryEntityLog.Add(log);
                }
            }
        }
        catch (Exception ex)
        {
            await LogException(ex);
        }
    }
    public async Task LogDeleteHistory<T>(T data) where T : Entity
    {
        try
        {
            
            var log = new EntityLog()
            {
                TableId = data.Id,
                TableName = typeof(T).Name,
                AdminUserId = _userAccessor.AdminUserId,
                Changes = EntityMetaData.GetCurrent(data),
                LogType = Domain.Enums.LogType.DELETE,
                ClientIP = _userAccessor.ClientIP
            };
            if (_globalUnitOfWork.IsTransactional())
            {
                _EntityLogs.Add(log);
            }
            else
            {
                await _repositoryEntityLog.Add(log);
            }
        }
        catch (Exception ex)
        {
            await LogException(ex);
        }
    }
    public async Task WriteEntityHistories()
    {
        try
        {
            foreach (var item in _EntityLogs)
            {
                await _repositoryEntityLog.Add(item);
            }
            _EntityLogs.Clear();
        }
        catch (Exception ex)
        {
            await LogException(ex);
        }
    }
    public async Task ClearEntityHistories()
    {
        try
        {
            _EntityLogs.Clear();
        }
        catch (Exception ex)
        {
            await LogException(ex);
        }
    }

    private EntityLog? GetEntityLog<T>(T data, T? old) where T : Entity
    {
        var log = new EntityLog()
        {
            TableId = data.Id,
            TableName = typeof(T).Name,
            AdminUserId = _userAccessor.AdminUser?.Id,
            Changes = null,
            LogType = data.Deleted ? Domain.Enums.LogType.DELETE : Domain.Enums.LogType.INSERT,
            ClientIP = _userAccessor.ClientIP
        };
        if (log.LogType == Domain.Enums.LogType.DELETE)
        {
            log.Changes = null;//EntityMetaData.GetCurrent(data);
        }
        else
        {
            if (old != null)
            {
                log.LogType = Domain.Enums.LogType.UPDATE;
                log.Changes = EntityMetaData.GetChanges(data, old);
                if (string.IsNullOrEmpty(log.Changes))
                {
                    return default;
                }
            }
        }
        return log;
    }
    private static string GetErrorMessage(Exception ex, int level)
    {
        var message = "";
        if (level > 0)
        {
            message += string.Concat(Enumerable.Repeat(Environment.NewLine, 2));
            message += string.Concat(Enumerable.Repeat("-", 80)) + Environment.NewLine;
        }
        message += ex.Message + Environment.NewLine;
        message += ex.StackTrace + Environment.NewLine;
        if (ex.InnerException != null)
        {
            message += GetErrorMessage(ex.InnerException, level + 1);
        }

        return message;
    }
    private static string GetErrorString(Exception ex)
    {
        var message = ex.Message;
        if (ex.InnerException != null)
        {
            message += Environment.NewLine + GetErrorString(ex.InnerException);
        }
        return message;
    }
}

public class EntityMetaData
{
    private static readonly List<ClassField> _TypeFields = [];
    private static readonly object _Locker = new();
    private static readonly string[] _IgnoredFields = ["CreateDate", "UpdateDate", "Deleted"];
    private static readonly Type[] _IgnoredClass = [typeof(EntityLog), typeof(ErrorLog)];
    private static readonly Dictionary<Type, string[]> _ClassIgonredFields = new()
    {
        //{ typeof(Customer), new string[] { "LastLoginDate", "LastIpAddress" } }
    };

    public static string? GetChanges<T>(T data, T old)
    {
        var fields = GetFields<T>();
        var jData = new JsonObject();
        var changed = false;
        foreach (var item in fields)
        {
            var value = item.Value.Invoke(data);
            var oldValue = item.Value.Invoke(old);
            if (!Equals(value, oldValue))
            {
                changed = true;
                jData[item.Key] = new JsonArray();
                ((JsonArray)jData[item.Key]!).Add(oldValue);
                ((JsonArray)jData[item.Key]!).Add(value);
            }
        }
        if (!changed)
        {
            return default;
        }
        return jData.ToString();
    }

    public static string GetCurrent<T>(T data)
    {
        var fields = GetFields<T>();
        var jData = new JsonObject();
        foreach (var item in fields)
        {
            var value = item.Value.Invoke(data);
            if (value != null)
            {
                jData[item.Key] = JsonValue.Create(data);
            }
        }
        return jData.ToString();
    }

    private static Dictionary<string, Func<object?, object?>> GetFields<T>()
    {
        lock (_Locker)
        {
            var classType = typeof(T);
            if (_IgnoredClass.Contains(classType))
            {
                return [];
            }
            var type = _TypeFields.FirstOrDefault(a => a.ClassType == classType);
            if (type == null)
            {
                type = new ClassField(classType);
                foreach (var item in type.ClassType.GetProperties())
                {
                    if (!item.PropertyType.IsValueType())
                    {
                        continue;
                    }
                    if (_IgnoredFields.Contains(item.Name))
                    {
                        continue;
                    }
                    if (_ClassIgonredFields.TryGetValue(classType, out string[]? value) && value.Contains(item.Name))
                    {
                        continue;
                    }

                    var getter = GetPropGetter(item);
                    if (getter != null)
                    {
                        type.Fields[item.Name] = getter;
                    }
                }
            }
            return type.Fields;
        }
    }

    private static Func<object?, object?>? GetPropGetter(PropertyInfo propertyInfo)
    {
        var data = Expression.Parameter(typeof(object), "data");
        var dataConverted = Expression.Convert(data, propertyInfo.DeclaringType!);
        if (propertyInfo.GetMethod != null)
        {
            var body = Expression.Call(dataConverted, propertyInfo.GetGetMethod()!);
            return Expression.Lambda<Func<object?, object?>>(Expression.Convert(body, typeof(object)), data).Compile();
        }
        return null;
    }

    internal class ClassField(Type type)
    {
        public Type ClassType { get; set; } = type;
        public Dictionary<string, Func<object?, object?>> Fields { get; set; } = [];
    }
}
