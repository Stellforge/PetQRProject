using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;

namespace SimpleProject.Domain;

public static partial class Extensions
{
    public static readonly MethodInfo? AnyMethod = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(a => a.Name == nameof(Enumerable.Any) && a.IsGenericMethod && a.GetGenericArguments().Length == 1 && a.GetParameters().Length == 2)
            .Where(a => a.GetParameters()[0].ParameterType == typeof(IEnumerable<>).MakeGenericType(a.GetGenericArguments()[0]))
            .Where(a => a.GetParameters()[1].ParameterType == typeof(Func<,>).MakeGenericType(a.GetGenericArguments()[0], typeof(bool)))
            .FirstOrDefault();

    private static readonly System.Globalization.CultureInfo enCultureInfo = new("en-US");

    public static string Md5(this string content)
    {
        return content.Md5(Encoding.UTF8);
    }
    public static string Md5(this string content, Encoding encoding)
    {
        var data = encoding.GetBytes(content);
        var result = MD5.HashData(data);
        var sb = new StringBuilder();
        for (int i = 0; i < result.Length; i++)
        {
            sb.Append(result[i].ToString("X2"));
        }
        return sb.ToString();
    }
    public static string SHA1(this string content)
    {
        byte[] data = Encoding.UTF8.GetBytes(content);
        byte[] result = System.Security.Cryptography.SHA1.HashData(data);
        var sb = new StringBuilder();
        for (int i = 0; i < result.Length; i++)
        {
            sb.Append(result[i].ToString("X2"));
        }
        return sb.ToString();
    }
    public static Guid ConvertToGuid(this string content)
    {
        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(content)));
    }

    public static string ToUrl(this string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        var tmp = url.ToLower(enCultureInfo).Replace("ı", "i").Replace("ö", "o").Replace("ü", "u").Replace("ş", "s").Replace("ğ", "g").Replace("ç", "c").Replace("é", "e").Replace(" ", "-");
        tmp = UrlClearRegex().Replace(tmp, "");
        while (tmp.Contains("--", StringComparison.InvariantCultureIgnoreCase))
        {
            tmp = tmp.Replace("--", "-");
        }
        tmp = tmp.TrimEnd('-');
        return tmp;
    }

    public static string GenerateKeyword(int length, bool number = false)
    {
        string password = string.Empty;

        var rnd = new Random();
        for (int charCounter = 0; charCounter < length; charCounter++)
        {
            int keyValue;
            if (number)
            {
                keyValue = rnd.Next(0, 10);
                password += keyValue.ToString();
                continue;
            }
            keyValue = rnd.Next(65, 90);
            password += Convert.ToChar(keyValue);
        }
        return password;
    }
    public static bool IsValueType(this Type type)
    {
        return type.IsValueType || type.IsEnum || type.Equals(typeof(string)) || type.IsNullableType();
    }
    public static bool IsNullableType(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    public static string? GetDisplayName(this Enum enumValue)
    {
        var type = enumValue.GetType();
        if (type.GetCustomAttributes<FlagsAttribute>().Any())
        {
            var items = new List<string?>();
            foreach (Enum item in Enum.GetValues(type))
            {
                if (Convert.ToInt32(item) == 0)
                {
                    continue;
                }
                if (enumValue.HasFlag(item))
                {
                    items.Add(type.GetMember(item.ToString()).First().GetCustomAttribute<DisplayAttribute>()?.GetName());
                }
            }
            return string.Join(",", items);
        }
        return type.GetMember(enumValue.ToString()).First().GetCustomAttribute<DisplayAttribute>()?.GetName();
    }
    public static string? GetDisplayName(this PropertyInfo property)
    {
        var displayAttr = property.GetCustomAttribute<DisplayAttribute>(true);
        if (displayAttr != null)
        {
            return displayAttr.GetName();
        }
        else
        {
            var displayNameAttr = property.GetCustomAttribute<DisplayNameAttribute>(true);
            if (displayNameAttr != null)
            {
                return displayNameAttr.DisplayName;
            }
        }
        return property.Name;
    }
    public static List<T> GetFlagList<T>(this T value) where T : Enum
    {
        var list = new List<T>();
        foreach (T item in Enum.GetValues(typeof(T)))
        {
            if (item.HasFlag(value))
            {
                list.Add(item);
            }
        }
        return list;
    }
    public static object? GetDefaultValue(this Type type)
    {
        return Expression.Lambda<Func<object?>>(Expression.Convert(Expression.Default(type), typeof(object))).Compile().Invoke();
    }
    public static Func<T, object?>? GetPropGetter<T>(this PropertyInfo propertyInfo)
    {
        var data = Expression.Parameter(typeof(T), "data");
        if (propertyInfo.GetMethod != null)
        {
            var method = propertyInfo.GetGetMethod();
            if (method != null)
            {
                var body = Expression.Call(data, method);
                return Expression.Lambda<Func<T, object?>>(Expression.Convert(body, typeof(object)), data).Compile();
            }
        }
        return null;
    }
    public static Action<T, object?>? GetPropSetter<T>(this PropertyInfo propertyInfo)
    {
        var data = Expression.Parameter(typeof(T), "data");
        if (propertyInfo.DeclaringType != null)
        {
            var dataConverted = Expression.Convert(data, propertyInfo.DeclaringType);
            var value = Expression.Parameter(typeof(object), "value");
            var valueConverted = Expression.Convert(value, propertyInfo.PropertyType);
            if (propertyInfo.SetMethod != null)
            {
                var method = propertyInfo.GetSetMethod();
                if (method != null)
                {
                    var body = Expression.Call(dataConverted, method, valueConverted);
                    return Expression.Lambda<Action<T, object?>>(body, data, value).Compile();
                }
            }
        }
        return null;
    }

    public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
    {
        var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);
        var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
        return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
    }
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.And);
    }
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.Or);
    }

    public static IEnumerable<T> DynamicCast<T>(this IEnumerable source)
    {
        foreach (dynamic current in source)
        {
            yield return (T)(current);
        }
    }

    public static Expression<Func<TEntity, TDto>> GetConvertExpression<TEntity, TDto>()
    {
        var arg = Expression.Parameter(typeof(TEntity), "a");
        return Expression.Lambda<Func<TEntity, TDto>>(Expression.Convert(arg, typeof(TDto)), arg);
    }
    public static MemberExpression? FindMemberExpression(this Expression expression)
    {
        if (expression is MemberExpression memberExpression)
        {
            return memberExpression;
        }
        else if (expression is UnaryExpression unary)
        {
            return unary.Operand.FindMemberExpression();
        }
        else if (expression is ConditionalExpression conditional)
        {
            var falseResult = conditional.IfFalse.FindMemberExpression();
            if (falseResult != null)
            {
                return falseResult;
            }
            var trueResult = conditional.IfTrue.FindMemberExpression();
            if (trueResult != null)
            {
                return trueResult;
            }
        }
        return default;
    }

    public static Expression<Func<TEntity, TEntity>> GetSelect<TDto, TEntity>(this IEnumerable<ExcelColumn<TDto>> columns, Expression<Func<TEntity, TEntity>>? additionalSelect = null) where TDto : new()
    {
        var fields = new List<string>();
        foreach (var item in columns)
        {
            if (!string.IsNullOrEmpty(item.Name))
            {
                fields.Add(item.Name);
            }
        }
        return GetSelectExpression(fields, additionalSelect);
    }
    public static Query<T> GetQuery<T>(this GridRequest request, Expression<Func<T, T>>? additionalSelect = null) where T : Entity
    {
        var query = new Query<T>()
        {
            Orders = [],
            Filters = [],
            Top = 20,
            Skip = 0
        };
        if (request == null)
        {
            return query;
        }

        if (request != null)
        {
            query.Top = request.PageSize;
            query.Skip = (request.Page - 1) * request.PageSize;
        }

        if (!string.IsNullOrEmpty(request!.Sorting))
        {
            var sorting = request.Sorting.Split(":");
            var property = sorting.First();
            var parameterExpression = Expression.Parameter(typeof(T), "a");
            var expression = GetPropertySelector(property, parameterExpression);
            if (expression != null)
            {
                query.Orders.Add((Expression.Lambda(expression, parameterExpression), sorting.Last() == "ASC"));
            }
        }
        if (request.Filters != null)
        {
            foreach (var item in request.Filters)
            {
                if (string.IsNullOrEmpty(item.Field))
                {
                    continue;
                }
                var expression = GetFilterExpression<T>(item);
                if (expression != null)
                {
                    query.Filters.Add(expression);
                }
            }
        }
        if (request.Fields != null && request.Fields.Count != 0)
        {
            query.Select = GetSelectExpression([.. request.Fields], additionalSelect); //fields bozulmasin diye ToList() yapildi.
        }
        return query;
    }

    private static Expression<Func<T, bool>>? GetFilterExpression<T>(GridFilter filter)
    {
        if (string.IsNullOrEmpty(filter.Field) || string.IsNullOrEmpty(filter.Value))
        {
            return default;
        }
        var expressions = new List<Expression>();
        var arg = Expression.Parameter(typeof(T), "a");
        if (filter.Field.Contains(','))
        {
            foreach (var item in filter.Field.Split([','], StringSplitOptions.RemoveEmptyEntries))
            {
                var exp = GetPropertySelector(item, arg);
                if (exp != null)
                {
                    expressions.Add(exp);
                }
            }
        }
        else
        {
            var exp = GetPropertySelector(filter.Field, arg);
            if (exp != null)
            {
                expressions.Add(exp);
            }
        }

        if (expressions == null || expressions.Count == 0 || expressions.First() is not MemberExpression)
        {
            return default;
        }

        var validExpressions = new List<Expression>();
        var values = new List<IList>();
        foreach (var expression in expressions)
        {
            var propType = ((MemberExpression)expression).Type;
            bool isNullable = propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (Activator.CreateInstance(typeof(List<>).MakeGenericType([propType])) is not IList value)
            {
                continue;
            }

            var fieldType = isNullable ? Nullable.GetUnderlyingType(propType)! : propType;
            if (fieldType == typeof(string))
            {
                if (filter.Value.Contains(','))
                {
                    var list = filter.Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (list.Length > 0)
                    {
                        foreach (var item in list)
                        {
                            value.Add(item);
                        }
                    }
                }
                else
                {
                    value.Add(filter.Value);
                }
            }
            else
            {
                if (fieldType.IsEnum)
                {
                    if (filter.Value.Contains(','))
                    {
                        var list = filter.Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (list.Length > 0)
                        {
                            foreach (var item in list)
                            {
                                if (int.TryParse(item, out int intValue))
                                {
                                    if (TryEnumToObject(intValue, fieldType, out object? result))
                                    {
                                        value.Add(result);
                                    }
                                }
                                else
                                {
                                    if (TryEnumParse(item, fieldType, out object? result))
                                    {
                                        value.Add(result);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (int.TryParse(filter.Value, out int intValue))
                        {
                            if (TryEnumToObject(intValue, fieldType, out object? result))
                            {
                                value.Add(result);
                            }
                        }
                        else
                        {
                            if (TryEnumParse(filter.Value, fieldType, out object? result))
                            {
                                value.Add(result);
                            }
                        }
                    }
                }
                else
                {
                    if (filter.Value.Contains(','))
                    {
                        var list = filter.Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (list.Length > 0)
                        {
                            foreach (var item in list)
                            {
                                if (TryChange(item, fieldType, out object? result))
                                {
                                    value.Add(result);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (TryChange(filter.Value, fieldType, out object? result))
                        {
                            value.Add(result);
                        }
                    }
                }
            }

            if (value.Count > 0)
            {
                validExpressions.Add(expression);
                values.Add(value);
            }
        }

        if (values.Count == 0)
        {
            return default;
        }

        var expressionBody = validExpressions[0];
        var expressionValue = values[0];
        var expressionPropType = expressionValue.GetType().GenericTypeArguments.First();
        var finalExpression = GetSearchExpression(expressionBody, expressionPropType, expressionValue.Count == 1 ? expressionValue[0] : expressionValue, filter.Operant);
        for (int i = 1; i < validExpressions.Count; i++)
        {
            expressionBody = validExpressions[i];
            expressionValue = values[i];
            expressionPropType = expressionValue.GetType().GenericTypeArguments.First();
            var secondExpression = GetSearchExpression(expressionBody, expressionPropType, expressionValue.Count == 1 ? expressionValue[0] : expressionValue, filter.Operant);
            finalExpression = Expression.Or(finalExpression, secondExpression);
        }
        return Expression.Lambda<Func<T, bool>>(finalExpression, arg);
    }
    private static Expression GetSearchExpression(Expression expression, Type propType, object? value, string? operant = "=")
    {
        Expression? constant;
        if (value is IList)
        {
            constant = Expression.Constant(value, value.GetType());
        }
        else
        {
            constant = Expression.Constant(value, propType);
        }

        if (propType != typeof(string) && (operant == "*" || operant == "^" || operant == "$" || operant == "!*"))
        {
            operant = "=";
        }

         if (propType == typeof(DateTime))
        {
            expression = Expression.Property(expression, "Date");
            // expression = Expression.Quote(Expression.Lambda(Expression.Property(expression, "Date"), argExpr));
        }
        else if (propType == typeof(DateTime?))
        {
            expression = Expression.Property(Expression.Property(expression, "Value"), "Date");
        }

        Expression predicateBody;
        switch (operant)
        {
            case "=":
                if (value is IList)
                {
                    var arg = Expression.Parameter(propType, "b");
                    var inner = Expression.Lambda(Expression.Equal(expression, arg), arg);
                    if (AnyMethod == null)
                    {
                        throw new ArgumentException("AnyMethod");
                    }
                    predicateBody = Expression.Call(AnyMethod.MakeGenericMethod(propType), constant, inner);
                }
                else
                {
                    predicateBody = Expression.Equal(expression, constant);
                }
                break;
            case "!=":
                if (value is IList)
                {
                    var arg = Expression.Parameter(propType, "b");
                    var inner = Expression.Lambda(Expression.Not(Expression.Equal(expression, arg)), arg);
                    if (AnyMethod == null)
                    {
                        throw new ArgumentException("AnyMethod");
                    }
                    predicateBody = Expression.Call(AnyMethod.MakeGenericMethod(propType), constant, inner);
                }
                else
                {
                    predicateBody = Expression.Not(Expression.Equal(expression, constant));
                }
                break;
            case ">":
                if (value is IList)
                {
                    var arg = Expression.Parameter(propType, "b");
                    var inner = Expression.Lambda(Expression.GreaterThan(arg, expression), arg);
                    if (AnyMethod == null)
                    {
                        throw new ArgumentException("AnyMethod");
                    }
                    predicateBody = Expression.Call(AnyMethod.MakeGenericMethod(propType), constant, inner);
                }
                else
                {
                    predicateBody = Expression.GreaterThan(expression, constant);
                }
                break;
            case ">=":
                if (value is IList)
                {
                    var arg = Expression.Parameter(propType, "b");
                    var inner = Expression.Lambda(Expression.GreaterThanOrEqual(arg, expression), arg);
                    if (AnyMethod == null)
                    {
                        throw new ArgumentException("AnyMethod");
                    }
                    predicateBody = Expression.Call(AnyMethod.MakeGenericMethod(propType), constant, inner);
                }
                else
                {
                    predicateBody = Expression.GreaterThanOrEqual(expression, constant);
                }
                break;
            case "<":
                if (value is IList)
                {
                    var arg = Expression.Parameter(propType, "b");
                    var inner = Expression.Lambda(Expression.LessThan(arg, expression), arg);
                    if (AnyMethod == null)
                    {
                        throw new ArgumentException("AnyMethod");
                    }
                    predicateBody = Expression.Call(AnyMethod.MakeGenericMethod(propType), constant, inner);
                }
                else
                {
                    predicateBody = Expression.LessThan(expression, constant);
                }
                break;
            case "<=":
                if (value is IList)
                {
                    var arg = Expression.Parameter(propType, "b");
                    var inner = Expression.Lambda(Expression.LessThanOrEqual(arg, expression), arg);
                    if (AnyMethod == null)
                    {
                        throw new ArgumentException("AnyMethod");
                    }
                    predicateBody = Expression.Call(AnyMethod.MakeGenericMethod(propType), constant, inner);
                }
                else
                {
                    predicateBody = Expression.LessThanOrEqual(expression, constant);
                }
                break;
            case "^":
                if (value is IList)
                {
                    var arg = Expression.Parameter(propType, "b");
                    var method = typeof(string).GetMethod("StartsWith", [typeof(string)]) ?? throw new ArgumentException("StartsWith");
                    var inner = Expression.Lambda(Expression.Call(expression, method, arg), arg);
                    if (AnyMethod == null)
                    {
                        throw new ArgumentException("AnyMethod");
                    }
                    predicateBody = Expression.Call(AnyMethod.MakeGenericMethod(propType), constant, inner);
                }
                else
                {
                    var method = typeof(string).GetMethod("StartsWith", [typeof(string)]) ?? throw new ArgumentException("StartsWith");
                    predicateBody = Expression.Call(expression, method, constant);
                }
                break;
            case "$":
                if (value is IList)
                {
                    var arg = Expression.Parameter(propType, "b");
                    var method = typeof(string).GetMethod("EndsWith", [typeof(string)]) ?? throw new ArgumentException("EndsWith");
                    var inner = Expression.Lambda(Expression.Call(expression, method, arg), arg);
                    if (AnyMethod == null)
                    {
                        throw new ArgumentException("AnyMethod");
                    }
                    predicateBody = Expression.Call(AnyMethod.MakeGenericMethod(propType), constant, inner);
                }
                else
                {
                    var method = typeof(string).GetMethod("EndsWith", [typeof(string)]) ?? throw new ArgumentException("EndsWith");
                    predicateBody = Expression.Call(expression, method, constant);
                }
                break;
            case "!*":
                if (value is IList)
                {
                    var arg = Expression.Parameter(propType, "b");
                    var method = typeof(string).GetMethod("Contains", [typeof(string)]) ?? throw new ArgumentException("Contains");
                    var inner = Expression.Lambda(Expression.Not(Expression.Call(expression, method, arg)), arg);
                    if (AnyMethod == null)
                    {
                        throw new ArgumentException("AnyMethod");
                    }
                    predicateBody = Expression.Call(AnyMethod.MakeGenericMethod(propType), constant, inner);
                }
                else
                {
                    var method = typeof(string).GetMethod("Contains", [typeof(string)]) ?? throw new ArgumentException("Contains");
                    predicateBody = Expression.Not(Expression.Call(expression, method, constant));
                }
                break;
            case "*":
                if (value is IList)
                {
                    var arg = Expression.Parameter(propType, "b");
                    var method = typeof(string).GetMethod("Contains", [typeof(string)]) ?? throw new ArgumentException("Contains");
                    var inner = Expression.Lambda(Expression.Call(expression, method, arg), arg);
                    if (AnyMethod == null)
                    {
                        throw new ArgumentException("AnyMethod");
                    }
                    predicateBody = Expression.Call(AnyMethod.MakeGenericMethod(propType), constant, inner);
                }
                else
                {
                    var method = typeof(string).GetMethod("Contains", [typeof(string)]) ?? throw new ArgumentException("Contains");
                    predicateBody = Expression.Call(expression, method, constant);
                }
                break;
            default:
                throw new Exception("Not supported operator");
        }

        return predicateBody;
    }

    private static Expression<Func<T, T>> GetSelectExpression<T>(List<string> fields, Expression<Func<T, T>>? additionalSelect)
    {
        var arg = Expression.Parameter(typeof(T), "a");
        var newExpression = Expression.New(typeof(T));

        var bindings = new List<MemberBinding>();
        if (additionalSelect != null && additionalSelect.Body is MemberInitExpression)
        {
            arg = additionalSelect.Parameters.First();
        }

        if (fields != null && fields.Count > 0)
        {
            bindings = GetMemberBindings<T>(typeof(T), fields, arg);
        }

        if (additionalSelect != null && additionalSelect.Body is MemberInitExpression)
        {
            if (additionalSelect.Body is MemberInitExpression additionalInit)
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
    private static List<MemberBinding> GetMemberBindings<T>(Type returnType, List<string> fields, ParameterExpression arg)
    {
        return GetMemberBindings("", typeof(T), returnType, fields, arg);
    }
    private static List<MemberBinding> GetMemberBindings(string fullName, Type sourceType, Type targetType, List<string> fields, ParameterExpression arg)
    {
        var bindFields = new List<string>();
        var bindings = new List<MemberBinding>();
        var targetProps = targetType.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        var sourceProps = sourceType.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        foreach (var item in fields.Where(a => string.IsNullOrEmpty(fullName) || a.StartsWith(fullName + ".")))
        {
            if (bindFields.Contains(item))
            {
                continue;
            }
            //bindFields.Select(a => a);
            var propName = string.IsNullOrEmpty(fullName) ? item : item[(fullName.Length + 1)..];
            if (propName.Contains('.'))
            {
                var objPropName = propName.Split('.').First();
                var targetPropInfo = targetProps.FirstOrDefault(a => a.Name.Equals(objPropName, StringComparison.OrdinalIgnoreCase));
                var sourcePropInfo = sourceProps.FirstOrDefault(a => a.Name.Equals(objPropName, StringComparison.OrdinalIgnoreCase));
                if (targetPropInfo != null && sourcePropInfo != null)
                {
                    var objFullName = string.IsNullOrEmpty(fullName) ? objPropName : fullName + "." + objPropName;
                    var objFields = fields.Where(a => a.StartsWith(objFullName + ".")).ToList();

                    if (targetPropInfo.PropertyType.IsGenericType && targetPropInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                        && sourcePropInfo.PropertyType.IsGenericType && sourcePropInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var targetGenericType = targetPropInfo.PropertyType.GenericTypeArguments.First();
                        var sourceGenericType = sourcePropInfo.PropertyType.GenericTypeArguments.First();
                        var subArg = Expression.Parameter(sourceGenericType, "b");
                        var expression = GetNewExpression("", sourceGenericType, targetGenericType, objFields.Select(a => a[(objFullName + ".").Length..]).ToList(), subArg);
                        var propExpression = GetPropertySelector(objFullName, arg);
                        if (propExpression != null)
                        {
                            var selectLamda = Expression.Lambda(expression, subArg);
                            var selectExpression = Expression.Call(typeof(Enumerable), nameof(Enumerable.Select), [sourceGenericType, targetGenericType], propExpression, Expression.Lambda(expression, subArg));
                            var toListExpression = Expression.Call(typeof(Enumerable), nameof(Enumerable.ToList), [targetGenericType], selectExpression);
                            bindings.Add(Expression.Bind(targetPropInfo, toListExpression));
                        }
                    }
                    else
                    {
                        var propInfo = sourceProps.FirstOrDefault(a => a.Name.Equals(objPropName + "Id", StringComparison.OrdinalIgnoreCase));
                        if (propInfo != null)
                        {
                            var expression = GetNewExpression(objFullName, sourcePropInfo.PropertyType, targetPropInfo.PropertyType, objFields, arg);//sourceType
                            if (expression != null)
                            {
                                if (propInfo.PropertyType.IsNullableType())
                                {
                                    var propExpression = GetPropertySelector(objFullName, arg);
                                    if (propExpression != null)
                                    {
                                        var testExpression = Expression.Equal(propExpression, Expression.Constant(null));
                                        bindings.Add(Expression.Bind(targetPropInfo, Expression.Condition(testExpression, Expression.Default(targetPropInfo.PropertyType), expression)));
                                    }
                                }
                                else
                                {
                                    bindings.Add(Expression.Bind(targetPropInfo, expression));
                                }
                            }
                        }
                        else
                        {
                            var expression = GetNewExpression(objFullName, sourcePropInfo.PropertyType, targetPropInfo.PropertyType, objFields, arg);//sourceType
                            if (expression != null)
                            {
                                bindings.Add(Expression.Bind(targetPropInfo, expression));
                            }
                        }
                    }
                    bindFields.AddRange(objFields);
                }
            }
            else
            {
                var properyExpression = GetPropertySelector(item, arg);
                if (properyExpression != null)
                {
                    var propInfo = targetProps.FirstOrDefault(a => a.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));
                    if (propInfo != null)
                    {
                        bindings.Add(Expression.Bind(propInfo, properyExpression));
                    }
                }
                bindFields.Add(item);
            }
        }
        return bindings;
    }
    private static Expression? GetPropertySelector(string propertyName, ParameterExpression arg)
    {
        var type = arg.Type;
        if (propertyName.Contains('.'))
        {
            var properties = propertyName.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
            Expression expression = arg;
            foreach (var property in properties)
            {
                var pInfo = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (pInfo == null)
                {
                    return null;
                }
                expression = Expression.Property(expression, pInfo);
                type = pInfo.PropertyType;
            }
            return expression;
        }
        var propInfo = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (propInfo == null)
        {
            return null;
        }
        return Expression.Property(arg, propertyName);
    }
    private static MemberInitExpression GetNewExpression<T>(Type returnType, List<string> fields, ParameterExpression arg)
    {
        return GetNewExpression("", typeof(T), returnType, fields, arg);
    }
    private static MemberInitExpression GetNewExpression(string fullName, Type sourceType, Type targetType, List<string> fields, ParameterExpression arg)
    {
        var bindings = GetMemberBindings(fullName, sourceType, targetType, fields, arg);
        var newExpression = Expression.New(targetType);
        return Expression.MemberInit(newExpression, bindings);
    }

    private static bool TryChange(object value, Type type, out object? result)
    {
        result = null;
        try
        {
            if ("null".Equals(value?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                result = Activator.CreateInstance(typeof(Nullable<>).MakeGenericType(type));
                return true;
            }

            result = Convert.ChangeType(value, type);
            if (type == typeof(DateTime) && ((DateTime)result!).Date == ((DateTime)result))
            {
                result = DateTime.SpecifyKind((DateTime)result, DateTimeKind.Utc);
            }
            return true;
        }
        catch //(Exception ex)
        {
            return false;
        }
    }
    private static bool TryEnumParse(string value, Type type, out object? result)
    {
        result = null;
        try
        {
            result = Enum.Parse(type, value, true);
            return true;
        }
        catch //(Exception ex)
        {
            return false;
        }
    }
    private static bool TryEnumToObject(int value, Type type, out object? result)
    {
        result = null;
        try
        {
            result = Enum.ToObject(type, value);
            return true;
        }
        catch //(Exception ex)
        {
            return false;
        }
    }


    [GeneratedRegex("[^A-Za-z0-9\\-]+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlClearRegex();
}

public class ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map) : ExpressionVisitor
{
    private readonly Dictionary<ParameterExpression, ParameterExpression> map = map ?? [];

    public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
    {
        return new ParameterRebinder(map).Visit(exp);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
        if (map.TryGetValue(p, out ParameterExpression? replacement))
        {
            p = replacement;
        }
        return base.VisitParameter(p);
    }
}
