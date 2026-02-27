using PRN222_Assignment4_Option1.DataAccess.Entities;

namespace PRN222_Assignment4_Option1.BusinessLogic.Services;

public interface IExchangeRateService
{
    Task<List<ExchangeRate>> GetListAsync(DateTime? date, int skip, int take, CancellationToken ct = default);
    Task<ExchangeRate?> GetLatestAsync(CancellationToken ct = default);
    Task<decimal?> GetAverageRateByDateAsync(DateTime date, string? baseCurrency = null, string? targetCurrency = null, CancellationToken ct = default);
    Task FetchAndSaveLatestRateAsync(string baseCurrency, string targetCurrency, CancellationToken ct = default);
    Task FetchAndSaveRandomRateAsync(CancellationToken ct = default);
}

