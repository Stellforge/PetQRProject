using System.Linq.Expressions;
using System.Reflection;

namespace SimpleProject.Domain.Dtos;
public class ExcelColumn<T> where T : new()
{
    public string? Title { get; set; }
    public string? Name { get; set; }
    public Func<T, object?>? Select { get; set; }
    public Action<T, object?>? Set { get; set; }
    public ExcelDataType? DataType { get; set; }
    public Type? PropertyType { get; set; }
    public static List<ExcelColumn<T>> Columns
    {
        get
        {
            if (_Columns != null)
            {
                return _Columns;
            }
            _Columns = new List<ExcelColumn<T>>();
            foreach (var item in typeof(T).GetProperties().Where(a => !_IgnoredFields.Contains(a.Name) && a.PropertyType.IsValueType()))
            {
                _Columns.Add(new ExcelColumn<T>(item.Name));
            }
            var idColumn = _Columns.FirstOrDefault(a => a.Name == "Id");
            if (idColumn != null)
            {
                _Columns.Remove(idColumn);
                _Columns.Insert(0, idColumn);
            }
            return _Columns;
        }
    }

    public static T SetObjectValues(T data, ExcelData<T> excelData, List<ExcelColumn<T>>? columns)
    {
        columns ??= Columns;
        foreach (var colum in excelData.Columns)
        {
            var excelColumn = columns.FirstOrDefault(a => a.Name == colum);
            if (excelColumn != null && excelColumn.Set != null && excelColumn.Select != null)
            {
                excelColumn.Set.Invoke(data, excelColumn.Select.Invoke(excelData.Data));
            }
        }
        return data;
    }

    private static readonly string[] _IgnoredFields = ["CreateDate", "UpdateDate", "Deleted"];
    private static List<ExcelColumn<T>>? _Columns;

    public ExcelColumn()
    {

    }

    public ExcelColumn(string name)
    {
        Name = name;
        Title = name;
        var property = typeof(T).GetProperty(name);
        if (property != null)
        {
            Select = property.GetPropGetter<T>();
            Set = property.GetPropSetter<T>();
            Title = property.GetDisplayName();
            SetDataType(property.PropertyType);
        }
    }

    public ExcelColumn(string title, string name)
    {
        Title = title;
        Name = name;
        var property = typeof(T).GetProperty(name);
        if (property != null)
        {
            Select = property.GetPropGetter<T>();
            Set = property.GetPropSetter<T>();
            SetDataType(property.PropertyType);
        }
    }

    public ExcelColumn(Expression<Func<T, object>> field, string? title = null, Func<T, object?>? getValueFn = null, Action<T, object?>? setValueFn = null)
    {
        Title = title;
        var memberExpression = field.Body.FindMemberExpression();
        if (memberExpression != null)
        {
            Name = memberExpression.ToString().TrimStart((field.Parameters.First().ToString() + ".").ToCharArray());
            var property = memberExpression.Member as PropertyInfo;
            if (property != null)
            {
                if (string.IsNullOrEmpty(Title))
                {
                    Title = property.GetDisplayName();
                }
                SetDataType(property.PropertyType);
                Set = property.GetPropSetter<T>();
            }
        }

        if (getValueFn != null)
        {
            Select = getValueFn;
        }
        else
        {
            Select = field.Compile();
        }
        if (setValueFn != null)
        {
            Set = setValueFn;
        }
    }

    public object? Value(T data)
    {
        if (Select == null)
        {
            return default;
        }

        return Select.Invoke(data);
    }

    public void SetValue(T data, object value)
    {
        if (Set == null)
        {
            return;
        }
        Set.Invoke(data!, value);
    }

    private void SetDataType(Type type)
    {
        PropertyType = type;
        if (type == typeof(string))
        {
            DataType = ExcelDataType.STRING;
        }
        else
        {
            var nulllable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (nulllable)
            {
                type = Nullable.GetUnderlyingType(type)!;
            }

            DataType = ExcelDataType.STRING;
            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
            {
                DataType = ExcelDataType.NUMBER;
            }
            else if (type == typeof(decimal) || type == typeof(float) || type == typeof(double))
            {
                DataType = ExcelDataType.FLOAT;
            }
            else if (type == typeof(DateTime))
            {
                DataType = ExcelDataType.DATE;
            }
            else if (type == typeof(bool))
            {
                DataType = ExcelDataType.BOOLEAN;
            }
        }
    }
}

public enum ExcelDataType
{
    STRING,
    NUMBER,
    FLOAT,
    DATE,
    BOOLEAN
}

