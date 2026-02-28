using Microsoft.EntityFrameworkCore;
using PRN222_Assignment4_Option1.DataAccess.Data;
using PRN222_Assignment4_Option1.DataAccess.Entities;

namespace PRN222_Assignment4_Option1.DataAccess.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly AppDbContext _context;

    public ExchangeRateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ExchangeRate entity, CancellationToken ct = default)
    {
        await _context.ExchangeRates.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<ExchangeRate> entities, CancellationToken ct = default)
    {
        await _context.ExchangeRates.AddRangeAsync(entities, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<ExchangeRate>> GetByDateAsync(DateTime date, CancellationToken ct = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await _context.ExchangeRates
            .Where(e => e.CreatedAt >= start && e.CreatedAt < end)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<ExchangeRate?> GetLatestAsync(CancellationToken ct = default)
    {
        return await _context.ExchangeRates
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<ExchangeRate>> GetAllAsync(DateTime? startDate, DateTime? endDate, string? baseCurrency, string? targetCurrencies, int skip, int take, CancellationToken ct = default)
    {
        var query = _context.ExchangeRates.AsQueryable();

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(e => e.Timestamp >= start);

            if (!endDate.HasValue)
            {
                var singleDayEnd = start.AddDays(1);
                query = query.Where(e => e.Timestamp < singleDayEnd);
            }
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1);
            query = query.Where(e => e.Timestamp < end);
        }

        if (!string.IsNullOrWhiteSpace(baseCurrency))
        {
            query = query.Where(e => e.BaseCurrency == baseCurrency.Trim());
        }

        if (!string.IsNullOrWhiteSpace(targetCurrencies))
        {
            var symbols = targetCurrencies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (symbols.Length > 0)
            {
                query = query.Where(e => symbols.Contains(e.TargetCurrency));
            }
        }

        return await query
            .OrderByDescending(e => e.Timestamp)
            .ThenByDescending(e => e.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<decimal?> GetAverageRateByDateAsync(DateTime date, string? baseCurrency = null, string? targetCurrency = null, CancellationToken ct = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        var query = _context.ExchangeRates
            .Where(e => e.CreatedAt >= start && e.CreatedAt < end);
        if (!string.IsNullOrEmpty(baseCurrency))
            query = query.Where(e => e.BaseCurrency == baseCurrency);
        if (!string.IsNullOrEmpty(targetCurrency))
            query = query.Where(e => e.TargetCurrency == targetCurrency);
        return await query.AverageAsync(e => (decimal?)e.Rate, ct);
    }

    public async Task<int> GetCountAsync(DateTime? startDate, DateTime? endDate, string? baseCurrency, string? targetCurrencies, CancellationToken ct = default)
    {
        var query = _context.ExchangeRates.AsQueryable();

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(e => e.Timestamp >= start);

            if (!endDate.HasValue)
            {
                var singleDayEnd = start.AddDays(1);
                query = query.Where(e => e.Timestamp < singleDayEnd);
            }
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1);
            query = query.Where(e => e.Timestamp < end);
        }

        if (!string.IsNullOrWhiteSpace(baseCurrency))
        {
            query = query.Where(e => e.BaseCurrency == baseCurrency.Trim());
        }

        if (!string.IsNullOrWhiteSpace(targetCurrencies))
        {
            var symbols = targetCurrencies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (symbols.Length > 0)
            {
                query = query.Where(e => symbols.Contains(e.TargetCurrency));
            }
        }

        return await query.CountAsync(ct);
    }

    public async Task<List<WorkerLog>> GetWorkerLogsAsync(int count, CancellationToken ct = default)
    {
        return await _context.WorkerLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<bool> HasDataForDateAsync(DateTime date, CancellationToken ct = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await _context.ExchangeRates
            .AnyAsync(e => e.Timestamp >= start && e.Timestamp < end, ct);
    }
}
