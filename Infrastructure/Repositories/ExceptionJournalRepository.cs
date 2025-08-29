using Microsoft.EntityFrameworkCore;
using TreeManagementApi.Application.Interfaces;
using TreeManagementApi.Domain.Entities;
using TreeManagementApi.Infrastructure.Data;

namespace TreeManagementApi.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ExceptionJournal operations
/// </summary>
public class ExceptionJournalRepository : IExceptionJournalRepository
{
    private readonly TreeManagementDbContext _context;

    public ExceptionJournalRepository(TreeManagementDbContext context)
    {
        _context = context;
    }

    public async Task<ExceptionJournal> AddExceptionAsync(ExceptionJournal exceptionJournal)
    {
        _context.ExceptionJournals.Add(exceptionJournal);
        await _context.SaveChangesAsync();
        return exceptionJournal;
    }

    public async Task<ExceptionJournal?> GetExceptionAsync(long id)
    {
        return await _context.ExceptionJournals
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<ExceptionJournal>> GetRecentExceptionsAsync(int count = 100)
    {
        return await _context.ExceptionJournals
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExceptionJournal>> GetExceptionsByTypeAsync(string exceptionType, int count = 50)
    {
        return await _context.ExceptionJournals
            .Where(e => e.ExceptionType.Contains(exceptionType))
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToListAsync();
    }
}