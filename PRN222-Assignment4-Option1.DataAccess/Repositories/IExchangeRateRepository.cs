using PRN222_Assignment4_Option1.DataAccess.Entities;

namespace PRN222_Assignment4_Option1.DataAccess.Repositories;

public interface IExchangeRateRepository
{
    Task AddAsync(ExchangeRate entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ExchangeRate> entities, CancellationToken ct = default);
    Task<List<ExchangeRate>> GetByDateAsync(DateTime date, CancellationToken ct = default);
    Task<ExchangeRate?> GetLatestAsync(CancellationToken ct = default);
    Task<List<ExchangeRate>> GetAllAsync(DateTime? startDate, DateTime? endDate, string? baseCurrency, string? targetCurrencies, int skip, int take, CancellationToken ct = default);
    Task<decimal?> GetAverageRateByDateAsync(DateTime date, string? baseCurrency = null, string? targetCurrency = null, CancellationToken ct = default);
}
