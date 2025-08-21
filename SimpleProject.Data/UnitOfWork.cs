using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace SimpleProject.Data;
public interface IUnitOfWork : IDisposable
{
    bool IsTransactional();
    Task BeginTransaction();
    Task CommitTransaction();
    Task RollbackTransaction();
    IDbContextTransaction? GetTransaction();
    void ClearChanges();
}

public class UnitOfWork : IUnitOfWork
{
    private bool _disposed;
    private readonly DbContext _dbContext;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginTransaction()
    {
        _transaction ??= await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransaction()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            _transaction = null;
        }
        ClearChanges();
    }

    public async Task RollbackTransaction()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            _transaction = null;
        }
        ClearChanges();
    }

    public bool IsTransactional()
    {
        return _transaction != null;
    }

    public IDbContextTransaction? GetTransaction()
    {
        return _transaction;
    }

    public void ClearChanges()
    {
        _dbContext.ChangeTracker.Clear();
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _dbContext.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
    }
}