using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain;
using Microsoft.EntityFrameworkCore.Storage;

namespace SimpleProject.Data;

public interface IRepository<T> : IDisposable where T : Entity, new()
{
    Task Add(T entity);
    Task Delete(int id);
    Task Delete(T entity);
    Task Update(T entity, T? oldEntity = null);
    Task DeletePermanent(int id);

    Task<int> ExecuteUpdate(Expression<Func<T, bool>>? filter, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls);

    Task<T?> Get(Expression<Func<T, bool>>? filter, params string[] includes);
    Task<T?> Get(Expression<Func<T, bool>>? filter, Expression<Func<T, T>>? select);
    Task<T?> Get(Query<T>? query);

    Task<IEnumerable<T>> Query(Expression<Func<T, bool>>? filter, params string[] includes);
    Task<IEnumerable<T>> Query(Expression<Func<T, bool>>? filter, Expression<Func<T, T>>? select);
    Task<IEnumerable<T>> Query(Query<T>? query);
    Task<(IEnumerable<T> Data, int Total)> QueryWithTotal(Query<T> query);

    IQueryable<T> AsQueryable();

    Task<int> Count(Expression<Func<T, bool>>? filter);
    Task<int> Count(Query<T>? query);

    Task<bool> Any(Expression<Func<T, bool>>? filter);
    Task<bool> Any(Query<T>? query);

    Task<TResult> Max<TResult>(Expression<Func<T, bool>>? filter, Expression<Func<T, TResult>> selector);
    Task<TResult> Max<TResult>(Query<T>? query, Expression<Func<T, TResult>> selector);

    Task<TResult> Min<TResult>(Expression<Func<T, bool>>? filter, Expression<Func<T, TResult>> selector);
    Task<TResult> Min<TResult>(Query<T>? query, Expression<Func<T, TResult>> selector);

    Task<int> Sum(Expression<Func<T, bool>>? filter, Expression<Func<T, int>> selector);
    Task<int> Sum(Query<T>? query, Expression<Func<T, int>> selector);

    Task<decimal> Sum(Expression<Func<T, bool>>? filter, Expression<Func<T, decimal>> selector);
    Task<decimal> Sum(Query<T>? query, Expression<Func<T, decimal>> selector);

    Task<IEnumerable<TElement>> GroupBy<TKey, TElement>(Expression<Func<T, bool>>? filter, Expression<Func<T, TKey>> keySelector, Expression<Func<IGrouping<TKey, T>, TElement>> elementSelector);
    Task<IEnumerable<TElement>> GroupBy<TKey, TElement>(Query<T>? query, Expression<Func<T, TKey>> keySelector, Expression<Func<IGrouping<TKey, T>, TElement>> elementSelector);

    Task<IEnumerable<T>> ExecuteFromRaw(string sql, params object[] parameters);
    Task<int> Execute(string sql, DbParamCollection? parameters = null, bool isSp = false);
    Task<TResult?> ExecuteScalar<TResult>(string sql, DbParamCollection? parameters = null, bool isSp = false);
}

public class Repository<T>(DbContext dbContext, IUnitOfWork unitOfWork) : IRepository<T> where T : Entity, new()
{
    private readonly DbContext _dbContext = dbContext;
    private readonly DbSet<T> _dbSet = dbContext.Set<T>();
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private bool _disposed;

    public async Task Add(T entity)
    {
        if (entity.Id == 0)
        {
            throw new InvalidOperationException("Id değeri yanlış");
        }
        entity.Id = default;
        await _dbSet.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        DetachAllEntries();
    }
    public async Task Delete(int id)
    {
        var entity = await Get(a => a.Id.Equals(id)) ?? throw new BusException("Silinecek kayıt bulunamadı");
        await Delete(entity);
    }
    public async Task Delete(T entity)
    {
        var oldEntity = MemberWiseCloner<T>.Clone(entity);
        entity.Deleted = true;
        await Update(entity, oldEntity);
    }
    public async Task Update(T entity, T? oldEntity = null)
    {
        if (oldEntity != null)
        {
            var oldEntityClone = MemberWiseCloner<T>.Clone(oldEntity);
            _dbContext.Attach(oldEntityClone).CurrentValues.SetValues(entity);

            RepositoryHelper.SetUpdateIgnore<T>(_dbContext.Entry(oldEntityClone));
            if (_dbContext.Entry(oldEntityClone).State == EntityState.Modified)
            {
                oldEntityClone.UpdateDate = DateTime.UtcNow;
                entity.UpdateDate = oldEntityClone.UpdateDate;
            }
        }
        else
        {
            entity.UpdateDate = DateTime.UtcNow;
            _dbSet.Update(entity);
            RepositoryHelper.SetUpdateIgnore<T>(_dbContext.Entry(entity));
        }
        await _dbContext.SaveChangesAsync();

        DetachAllEntries();
    }
    public async Task DeletePermanent(int id)
    {
        var entity = new T() { Id = id };

        _dbContext.Attach(entity);
        _dbContext.Entry(entity).State = EntityState.Deleted;

        await _dbContext.SaveChangesAsync();

        DetachAllEntries();
    }

    public async Task<int> ExecuteUpdate(Expression<Func<T, bool>>? filter, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls)
    {
        return await GetExecutionQuery(new Query<T>(filter)).ExecuteUpdateAsync(setPropertyCalls);
    }

    public async Task<T?> Get(Expression<Func<T, bool>>? filter, params string[]? includes)
    {
        var query = new Query<T>(filter);
        if (includes != null)
        {
            query.Includes = [.. includes];
        }
        return await Get(query);
    }
    public async Task<T?> Get(Expression<Func<T, bool>>? filter, Expression<Func<T, T>>? select)
    {
        var query = new Query<T>(filter);
        if (select != null)
        {
            query.Select = select;
        }
        return await Get(query);
    }
    public async Task<T?> Get(Query<T>? query)
    {
        var data = GetExecutionQuery(query);
        return await data.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> Query(Expression<Func<T, bool>>? filter, params string[]? includes)
    {
        var query = new Query<T>(filter);
        if (includes != null)
        {
            query.Includes = [.. includes];
        }
        return await Query(query);
    }
    public async Task<IEnumerable<T>> Query(Expression<Func<T, bool>>? filter, Expression<Func<T, T>>? select)
    {
        var query = new Query<T>(filter);
        if (select != null)
        {
            query.Select = select;
        }
        return await Query(query);
    }
    public async Task<IEnumerable<T>> Query(Query<T>? query)
    {
        var data = GetExecutionQuery(query);
        return await data.ToListAsync();
    }
    public async Task<(IEnumerable<T> Data, int Total)> QueryWithTotal(Query<T> query)
    {
        var totalCount = 0;
        IQueryable<T> data = _dbSet;
        IEnumerable<T> list = [];
        if (query != null)
        {
            if (query.Filters != null)
            {
                foreach (var filter in query.Filters)
                {
                    data = data.Where(filter);
                }
            }
            data = data.Where(a => !a.Deleted);

            if (query.Top > 0)
            {
                totalCount = await data.CountAsync();
            }

            if (query.Includes != null)
            {
                foreach (var include in query.Includes)
                {
                    data = data.Include(include);
                }
            }
            if (query.Orders != null && query.Orders.Count != 0)
            {
                var firsOrder = query.Orders.First();
                data = firsOrder.Asc ? (IQueryable<T>)Queryable.OrderBy(data, (dynamic)firsOrder.Expression) : (IQueryable<T>)Queryable.OrderByDescending(data, (dynamic)firsOrder.Expression);
                foreach (var orderBy in query.Orders.Skip(1))
                {
                    data = orderBy.Asc ? (IQueryable<T>)Queryable.ThenBy((IOrderedQueryable<T>)data, (dynamic)orderBy.Expression) : (IQueryable<T>)Queryable.ThenByDescending((IOrderedQueryable<T>)data, (dynamic)orderBy.Expression);
                }
            }
            if (query.Select != null)
            {
                data = data.Select(query.Select);
            }
            if (query.Skip > 0)
            {
                data = data.Skip(query.Skip);
            }
            if (query.Top > 0)
            {
                data = data.Take(query.Top);
            }

            list = await data.ToListAsync();
            if (query.Top <= 0)
            {
                totalCount = list.Count();
            }
        }
        else
        {
            list = await data.ToListAsync();
            totalCount = list.Count();
        }
        return (list, totalCount);
    }

    public IQueryable<T> AsQueryable() => _dbSet.AsQueryable();

    public async Task<int> Count(Expression<Func<T, bool>>? filter)
    {
        IQueryable<T> data = _dbSet;
        if (filter != null)
        {
            data = data.Where(filter);
        }
        data = data.Where(a => !a.Deleted);
        return await data.CountAsync();
    }
    public async Task<int> Count(Query<T>? query)
    {
        IQueryable<T> data = _dbSet;
        if (query != null && query.Filters != null)
        {
            foreach (var item in query.Filters)
            {
                data = data.Where(item);
            }
        }
        data = data.Where(a => !a.Deleted);
        return await data.CountAsync();
    }

    public async Task<bool> Any(Expression<Func<T, bool>>? filter)
    {
        return await Any(new Query<T>(filter));
    }
    public async Task<bool> Any(Query<T>? query)
    {
        IQueryable<T> data = _dbSet;
        if (query != null && query.Filters != null)
        {
            foreach (var item in query.Filters)
            {
                data = data.Where(item);
            }
        }
        data = data.Where(a => !a.Deleted);
        return await data.AnyAsync();
    }

    public async Task<TResult> Max<TResult>(Expression<Func<T, bool>>? filter, Expression<Func<T, TResult>> selector)
    {
        IQueryable<T> data = _dbSet;
        if (filter != null)
        {
            data = data.Where(filter);
        }
        data = data.Where(a => !a.Deleted);
        return await data.MaxAsync(selector);
    }
    public async Task<TResult> Max<TResult>(Query<T>? query, Expression<Func<T, TResult>> selector)
    {
        IQueryable<T> data = _dbSet;
        if (query != null && query.Filters != null)
        {
            foreach (var item in query.Filters)
            {
                data = data.Where(item);
            }
        }
        data = data.Where(a => !a.Deleted);
        return await data.MaxAsync(selector);
    }

    public async Task<TResult> Min<TResult>(Expression<Func<T, bool>>? filter, Expression<Func<T, TResult>> selector)
    {
        IQueryable<T> data = _dbSet;
        if (filter != null)
        {
            data = data.Where(filter);
        }
        data = data.Where(a => !a.Deleted);
        return await data.MinAsync(selector);
    }
    public async Task<TResult> Min<TResult>(Query<T>? query, Expression<Func<T, TResult>> selector)
    {
        IQueryable<T> data = _dbSet;
        if (query != null && query.Filters != null)
        {
            foreach (var item in query.Filters)
            {
                data = data.Where(item);
            }
        }
        data = data.Where(a => !a.Deleted);
        return await data.MinAsync(selector);
    }

    public async Task<int> Sum(Expression<Func<T, bool>>? filter, Expression<Func<T, int>> selector)
    {
        IQueryable<T> data = _dbSet;
        if (filter != null)
        {
            data = data.Where(filter);
        }
        data = data.Where(a => !a.Deleted);
        return await data.SumAsync(selector);
    }
    public async Task<int> Sum(Query<T>? query, Expression<Func<T, int>> selector)
    {
        IQueryable<T> data = _dbSet;
        if (query != null && query.Filters != null)
        {
            foreach (var item in query.Filters)
            {
                data = data.Where(item);
            }
        }
        data = data.Where(a => !a.Deleted);
        return await data.SumAsync(selector);
    }

    public async Task<decimal> Sum(Expression<Func<T, bool>>? filter, Expression<Func<T, decimal>> selector)
    {
        IQueryable<T> data = _dbSet;
        if (filter != null)
        {
            data = data.Where(filter);
        }
        data = data.Where(a => !a.Deleted);
        return await data.SumAsync(selector);
    }
    public async Task<decimal> Sum(Query<T>? query, Expression<Func<T, decimal>> selector)
    {
        IQueryable<T> data = _dbSet;
        if (query != null && query.Filters != null)
        {
            foreach (var item in query.Filters)
            {
                data = data.Where(item);
            }
        }
        data = data.Where(a => !a.Deleted);
        return await data.SumAsync(selector);
    }

    public async Task<IEnumerable<TElement>> GroupBy<TKey, TElement>(Expression<Func<T, bool>>? filter, Expression<Func<T, TKey>> keySelector, Expression<Func<IGrouping<TKey, T>, TElement>> elementSelector)
    {
        IQueryable<T> data = _dbSet;
        if (filter != null)
        {
            data = data.Where(filter);
        }
        data = data.Where(a => !a.Deleted);
        return await data.GroupBy(keySelector).Select(elementSelector).ToListAsync();
    }
    public async Task<IEnumerable<TElement>> GroupBy<TKey, TElement>(Query<T>? query, Expression<Func<T, TKey>> keySelector, Expression<Func<IGrouping<TKey, T>, TElement>> elementSelector)
    {
        IQueryable<T> data = _dbSet;
        if (query != null && query.Filters != null)
        {
            foreach (var item in query.Filters)
            {
                data = data.Where(item);
            }
        }
        data = data.Where(a => !a.Deleted);
        return await data.GroupBy(keySelector).Select(elementSelector).ToListAsync();
    }

    public async Task<IEnumerable<T>> ExecuteFromRaw(string sql, params object[] parameters)
    {
        return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();
    }
    public async Task<int> Execute(string sql, DbParamCollection? parameters = null, bool isSp = false)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var connectionOpened = false;
        int data;
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = sql;
            if (isSp)
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
            }
            if (_unitOfWork.IsTransactional())
            {
                command.Transaction = _unitOfWork.GetTransaction()?.GetDbTransaction();
            }
            if (parameters != null && parameters.Count > 0)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }
            if (connection.State.Equals(System.Data.ConnectionState.Closed))
            {
                await connection.OpenAsync();
                connectionOpened = true;
            }
            data = await command.ExecuteNonQueryAsync();
        }
        if (connectionOpened && connection.State.Equals(System.Data.ConnectionState.Open))
        {
            await connection.CloseAsync();
        }
        return data;
    }
    public async Task<TResult?> ExecuteScalar<TResult>(string sql, DbParamCollection? parameters = null, bool isSp = false)
    {
        var connection = _dbContext.Database.GetDbConnection();

        object? data;
        var connectionOpened = false;
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = sql;
            if (isSp)
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
            }
            if (_unitOfWork.IsTransactional())
            {
                command.Transaction = _unitOfWork.GetTransaction()?.GetDbTransaction();
            }
            if (parameters != null && parameters.Count > 0)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }
            if (connection.State.Equals(System.Data.ConnectionState.Closed))
            {
                await connection.OpenAsync();
                connectionOpened = true;
            }
            data = await command.ExecuteScalarAsync();
        }
        if (connectionOpened && connection.State.Equals(System.Data.ConnectionState.Open))
        {
            await connection.CloseAsync();
        }
        if (data == null || data == DBNull.Value)
        {
            return default;
        }
        return (TResult)Convert.ChangeType(data, typeof(TResult));
    }

    private IQueryable<T> GetExecutionQuery(Query<T>? query)
    {
        IQueryable<T> data = _dbSet;
        if (query != null)
        {
            if (query.Filters != null)
            {
                foreach (var filter in query.Filters)
                {
                    data = data.Where(filter);
                }
            }
            data = data.Where(a => !a.Deleted);

            if (query.Includes != null)
            {
                foreach (var include in query.Includes)
                {
                    data = data.Include(include);
                }
            }
            if (query.Orders != null && query.Orders.Count != 0)
            {
                var firsOrder = query.Orders.First();
                data = firsOrder.Asc ? (IQueryable<T>)Queryable.OrderBy(data, (dynamic)firsOrder.Expression) : (IQueryable<T>)Queryable.OrderByDescending(data, (dynamic)firsOrder.Expression);
                foreach (var orderBy in query.Orders.Skip(1))
                {
                    data = orderBy.Asc ? (IQueryable<T>)Queryable.ThenBy((IOrderedQueryable<T>)data, (dynamic)orderBy.Expression) : (IQueryable<T>)Queryable.ThenByDescending((IOrderedQueryable<T>)data, (dynamic)orderBy.Expression);
                }
            }
            if (query.Select != null)
            {
                data = data.Select(query.Select);
            }
            if (query.Skip > 0)
            {
                data = data.Skip(query.Skip);
            }
            if (query.Top > 0)
            {
                data = data.Take(query.Top);
            }
            return data;
        }
        else
        {
            data = data.Where(a => !a.Deleted);
            return data;
        }
    }

    public void Dispose() 
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        _unitOfWork.Dispose();
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private void DetachAllEntries()
    {
        foreach (var dbEntityEntry in _dbContext.ChangeTracker.Entries().ToList())
        {
            dbEntityEntry.State = EntityState.Detached;
        }
    }
}
