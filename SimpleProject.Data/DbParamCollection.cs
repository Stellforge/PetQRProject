using Microsoft.Data.SqlClient;
using System.Collections;
using System.Data.Common;
using System.Data;

namespace SimpleProject.Data;
public class DbParamCollection : CollectionBase, ICollection<DbParameter>
{
    private int _outputParameterIndex;

    public void Add(string parameterName, object? value)
    {
        DbParameter dbParameter = new SqlParameter(parameterName, value);
        dbParameter.Value ??= DBNull.Value;
        InnerList.Add(dbParameter);
    }
    public void Add(string parameterName, object? value, string typeName)
    {
        DbParameter dbParameter = new SqlParameter()
        {
            ParameterName = parameterName
        };
        value ??= DBNull.Value;
        dbParameter.Value = value;
        ((SqlParameter)dbParameter).TypeName = typeName;
        InnerList.Add(dbParameter);
    }
    public void Add(string parameterName, object? value, DbType dbType)
    {
        DbParameter dbParameter = new SqlParameter()
        {
            ParameterName = parameterName
        };
        value ??= DBNull.Value;
        dbParameter.Value = value;
        dbParameter.DbType = dbType;
        InnerList.Add(dbParameter);
    }

    public void AddOutput(string parameterName, DbType type, int size = 0)
    {
        var dbParameter = new SqlParameter()
        {
            ParameterName = parameterName,
            DbType = type,
            Direction = ParameterDirection.Output
        };

        if (size > 0)
        {
            dbParameter.Size = size;
        }

        if (type == DbType.Decimal)
        {
            dbParameter.Size = 18;
            dbParameter.Scale = 4;
        }
        _outputParameterIndex = InnerList.Add(dbParameter);
    }

    public Array ToArray()
    {
        return InnerList.ToArray();
    }

    public DbParameter? GetOutPutParameter(string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            return (DbParameter)InnerList[_outputParameterIndex]!;
        }
        foreach (DbParameter item in InnerList)
        {
            if (item.Direction == ParameterDirection.Output && item.ParameterName == name)
            {
                return item;
            }
        }
        return null;
    }

    public T? GetOutput<T>(string name = "")
    {
        var output = GetOutPutParameter(name);
        if (output != null && output.Value != null && output.Value != DBNull.Value)
        {
            var type = typeof(T);
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable)
            {
                return (T)Convert.ChangeType(output.Value, Nullable.GetUnderlyingType(type)!);
            }
            else
            {
                return (T)Convert.ChangeType(output.Value, type);
            }
        }
        return default;
    }

    public bool IsReadOnly => InnerList.IsReadOnly;

    public void Add(DbParameter item)
    {
        item.Value ??= DBNull.Value;
        InnerList.Add(item);
    }

    public bool Contains(DbParameter item)
    {
        return InnerList.Contains(item);
    }

    public void CopyTo(DbParameter[] array, int arrayIndex)
    {
        InnerList.CopyTo(array, arrayIndex);
    }

    public bool Remove(DbParameter item)
    {
        InnerList.Remove(item);
        return true;
    }

    IEnumerator<DbParameter> IEnumerable<DbParameter>.GetEnumerator()
    {
        return (IEnumerator<DbParameter>)InnerList.GetEnumerator();
    }
}
