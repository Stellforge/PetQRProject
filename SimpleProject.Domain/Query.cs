using System.Linq.Expressions;
using System.Reflection;

namespace SimpleProject.Domain;
public class Query<T>
{
    public int Top { get; set; }
    public int Skip { get; set; }
    public List<string>? Includes { get; set; }
    public List<Expression<Func<T, bool>>>? Filters { get; set; }
    public List<(LambdaExpression Expression, bool Asc)>? Orders { get; set; }
    public Expression<Func<T, T>>? Select { get; set; }

    public Query()
    {
    }

    public Query(Expression<Func<T, bool>>? filter = null, Expression<Func<T, T>>? select = null)
    {
        if (filter != null)
        {
            Filters = [filter];
        }
        if (select != null)
        {
            Select = select;
        }
    }

    public IEnumerable<T> Apply(IQueryable<T> data)
    {
        if (Filters != null)
        {
            foreach (var filter in Filters)
            {
                data = data.Where(filter);
            }
        }

        if (Orders != null && Orders.Count > 0)
        {
            var firsOrder = Orders.First();
            data = firsOrder.Asc ? (IQueryable<T>)Queryable.OrderBy(data, (dynamic)firsOrder.Expression) : (IQueryable<T>)Queryable.OrderByDescending(data, (dynamic)firsOrder.Expression);
            foreach (var orderBy in Orders.Skip(1))
            {
                data = orderBy.Asc ? (IQueryable<T>)Queryable.ThenBy((IOrderedQueryable<T>)data, (dynamic)orderBy.Expression) : (IQueryable<T>)Queryable.ThenByDescending((IOrderedQueryable<T>)data, (dynamic)orderBy.Expression);
            }
        }
        if (Select != null)
        {
            data = data.Select(Select);
        }
        if (Skip > 0)
        {
            data = data.Skip(Skip);
        }
        if (Top > 0)
        {
            data = data.Take(Top);
        }
        return data;
    }

    public IEnumerable<T> ApplyWithTotal(IEnumerable<T> data, out int total)
    {
        var set = data.AsQueryable();
        if (Filters != null)
        {
            foreach (var filter in Filters)
            {
                set = set.Where(filter);
            }
        }

        if (Orders != null && Orders.Count > 0)
        {
            var firsOrder = Orders.First();

            string command = firsOrder.Asc ? "OrderBy" : "OrderByDescending";

            var resultExpression = Expression.Call(typeof(Queryable), command, [typeof(T), firsOrder.Expression.Body.Type], set.Expression, Expression.Quote(firsOrder.Expression));
            set = (IOrderedQueryable<T>)set.Provider.CreateQuery<T>(resultExpression);

            foreach (var orderBy in Orders.Skip(1))
            {
                command = firsOrder.Asc ? "ThenBy" : "ThenByDescending";
                resultExpression = Expression.Call(typeof(Queryable), command, [typeof(T), firsOrder.Expression.Body.Type], set.Expression, Expression.Quote(firsOrder.Expression));
                set = (IOrderedQueryable<T>)set.Provider.CreateQuery<T>(resultExpression);
            }
        }
        if (Select != null)
        {
            set = set.Select(Select);
        }
        total = set.Count();
        if (Skip > 0)
        {
            set = set.Skip(Skip);
        }
        if (Top > 0)
        {
            set = set.Take(Top);
        }
        return [.. set];
    }

    public Expression<Func<T, bool>>? GetFilter()
    {
        Expression<Func<T, bool>>? filter = null;
        if (Filters != null && Filters.Count > 0)
        {
            filter = Filters.First();
            foreach (var item in Filters.Skip(1))
            {
                filter = filter.And(item);
            }
        }
        return filter;
    }

    public void SelectAll(Expression<Func<T, T>>? extra = null)
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
        Select = Expression.Lambda<Func<T, T>>(memberInitExpression, arg);
    }

    public void AddFilter(Expression<Func<T, bool>> filter)
    {
        Filters ??= [];
        Filters.Add(filter);
    }
    public void AddFilters(List<Expression<Func<T, bool>>> filters)
    {
        Filters ??= [];
        Filters.AddRange(filters);
    }

    public void AddSort<TKey>(Expression<Func<T, TKey>> expression, bool asc)
    {
        Orders ??= [];
        Orders.Add((expression, asc));
    }
    public void AddSelect(Expression<Func<T, T>> additionalSelect)
    {
        if (Select == null)
        {
            return;
        }

        if (Select.Body is MemberInitExpression && additionalSelect.Body is MemberInitExpression)
        {
            var arg = Select.Parameters.First();
            Select = Extensions.Compose(Select, additionalSelect, (exp1, exp2) => {
                var bindings = ((MemberInitExpression)exp1).Bindings.ToList();
                var additionalBindings = ((MemberInitExpression)exp2).Bindings.ToList();
                foreach (var additionalBinding in additionalBindings)
                {
                    var extBinding = bindings.FirstOrDefault(a => a.Member == additionalBinding.Member);
                    if (extBinding != null)
                    {
                        bindings.Remove(extBinding);
                    }
                    bindings.Add(additionalBinding);
                }
                var newExpression = Expression.New(typeof(T));
                return Expression.MemberInit(newExpression, bindings);
            });
        }
    }
}