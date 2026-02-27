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

    public async Task<List<ExchangeRate>> GetAllAsync(DateTime? date, int skip, int take, CancellationToken ct = default)
    {
        var query = _context.ExchangeRates.AsQueryable();
        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(e => e.CreatedAt >= start && e.CreatedAt < end);
        }
        return await query
            .OrderByDescending(e => e.CreatedAt)
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
}
