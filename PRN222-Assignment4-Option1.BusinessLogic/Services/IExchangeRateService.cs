using PRN222_Assignment4_Option1.BusinessLogic.Dtos;
using PRN222_Assignment4_Option1.DataAccess.Entities;

namespace PRN222_Assignment4_Option1.BusinessLogic.Services;

public interface IExchangeRateService
{
    Task<PagedResultDto<ExchangeRate>> GetListAsync(DateTime? startDate, DateTime? endDate, string? baseCurrency, string? targetCurrencies, int skip, int take, CancellationToken ct = default);
    Task<ExchangeRate?> GetLatestAsync(CancellationToken ct = default);
    Task<decimal?> GetAverageRateByDateAsync(DateTime date, string? baseCurrency = null, string? targetCurrency = null, CancellationToken ct = default);
    Task FetchAndSaveLatestRateAsync(string baseCurrency, string targetCurrency, CancellationToken ct = default);
    Task<ExchangeRate?> FetchAndSaveRandomRateAsync(CancellationToken ct = default);
    Task<ExchangeRate?> GenerateRandomRateAsync(CancellationToken ct = default);
    Task FetchAndSaveRangeRatesAsync(DateTime startDate, DateTime? endDate, string? baseCurrency = null, CancellationToken ct = default);
    Task<List<WorkerLog>> GetWorkerLogsAsync(int count, CancellationToken ct = default);
    /// <summary>Nếu ngày hôm nay chưa có data từ API thì gọi API và lưu. Trả về true nếu đã lưu mới.</summary>
    Task<bool> SyncTodayFromApiIfNeededAsync(CancellationToken ct = default);
}

