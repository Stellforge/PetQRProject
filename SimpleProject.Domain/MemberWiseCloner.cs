using System.Linq.Expressions;
using System.Reflection;

namespace SimpleProject.Domain;
public static class MemberWiseCloner<T>
{
    public static readonly Func<T, T> Clone;

    static MemberWiseCloner()
    {
        Clone = CreateClonerExpression().Compile();
    }

    private static Expression<Func<T, T>> CreateClonerExpression()
    {
        var cloneMethod = typeof(T).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
        ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "input");
        var cloneExpression = Expression.Lambda<Func<T, T>>(
            Expression.Convert(
                Expression.Condition(
                        Expression.ReferenceEqual(parameterExpression, Expression.Constant(null)),
                        Expression.Constant(null),
                        Expression.Call(
                            parameterExpression,
                            cloneMethod!
                        )),
                typeof(T)
            ),
            parameterExpression);

        return cloneExpression;
    }
}
