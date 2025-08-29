using Microsoft.EntityFrameworkCore.Storage;
using TreeManagementApi.Application.Interfaces;
using TreeManagementApi.Infrastructure.Data;

namespace TreeManagementApi.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for managing database transactions and repository coordination
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TreeManagementDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    // Lazy-loaded repositories
    private ITreeNodeRepository? _treeNodeRepository;
    private IExceptionJournalRepository? _exceptionJournalRepository;

    public UnitOfWork(TreeManagementDbContext context)
    {
        _context = context;
    }

    public ITreeNodeRepository TreeNodes
    {
        get
        {
            _treeNodeRepository ??= new TreeNodeRepository(_context);
            return _treeNodeRepository;
        }
    }

    public IExceptionJournalRepository ExceptionJournals
    {
        get
        {
            _exceptionJournalRepository ??= new ExceptionJournalRepository(_context);
            return _exceptionJournalRepository;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}