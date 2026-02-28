using Microsoft.Extensions.Logging;
using PRN222_Assignment4_Option1.BusinessLogic.Dtos;
using PRN222_Assignment4_Option1.DataAccess.Entities;
using PRN222_Assignment4_Option1.DataAccess.Repositories;

namespace PRN222_Assignment4_Option1.BusinessLogic.Services;

public sealed class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _repository;
    private readonly IExchangeRateApiService _apiService;
    private readonly ILogger<ExchangeRateService> _logger;

    public ExchangeRateService(
        IExchangeRateRepository repository, 
        IExchangeRateApiService apiService,
        ILogger<ExchangeRateService> logger)
    {
        _repository = repository;
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<PagedResultDto<ExchangeRate>> GetListAsync(DateTime? startDate, DateTime? endDate, string? baseCurrency, string? targetCurrencies, int skip, int take, CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(startDate, endDate, baseCurrency, targetCurrencies, skip, take, ct);
        var totalCount = await _repository.GetCountAsync(startDate, endDate, baseCurrency, targetCurrencies, ct);

        return new PagedResultDto<ExchangeRate>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public Task<ExchangeRate?> GetLatestAsync(CancellationToken ct = default)
    {
        return _repository.GetLatestAsync(ct);
    }

    public Task<decimal?> GetAverageRateByDateAsync(DateTime date, string? baseCurrency = null, string? targetCurrency = null, CancellationToken ct = default)
    {
        return _repository.GetAverageRateByDateAsync(date, baseCurrency, targetCurrency, ct);
    }

    public async Task FetchAndSaveLatestRateAsync(string baseCurrency, string targetCurrency, CancellationToken ct = default)
    {
        var data = await _apiService.GetLatestRateAsync(baseCurrency, targetCurrency, ct);
        if (data is null)
        {
            return;
        }

        var entity = new ExchangeRate
        {
            BaseCurrency = data.Base,
            TargetCurrency = data.Target,
            Rate = data.Mid,
            Timestamp = data.Timestamp,
            CreatedAt = data.Timestamp
        };

        await _repository.AddAsync(entity, ct);
    }

    public async Task<ExchangeRate?> FetchAndSaveRandomRateAsync(CancellationToken ct = default)
    {
        var entity = await GenerateRandomRateAsync(ct);
        if (entity is null)
            return null;

        await _repository.AddAsync(entity, ct);
        return entity;
    }

    public async Task<ExchangeRate?> GenerateRandomRateAsync(CancellationToken ct = default)
    {
        // 1. Kiểm tra bản ghi gần nhất trong database
        var latestRecord = await _repository.GetLatestAsync(ct);

        if (latestRecord == null)
        {
            // NẾU DATABASE TRỐNG: Gọi API lấy dữ liệu trong khoảng thời gian (mặc định USD)
            _logger.LogInformation("Database is empty. Fetching range rates from 2024-01-01.. with base USD...");
            
            // Theo yêu cầu: https://api.frankfurter.dev/v1/2024-01-01..?base=USD
            await FetchAndSaveRangeRatesAsync(new DateTime(2024, 1, 1), null, "USD", ct);
            
            // Sau khi fetch xong, lấy lại record mới nhất
            latestRecord = await _repository.GetLatestAsync(ct);
            if (latestRecord == null) return null;

            return latestRecord;
        }

        // NẾU ĐÃ CÓ DỮ LIỆU: Sử dụng hàm tự tính (Local Simulation)
        // Lấy danh sách 100 bản ghi gần nhất để có sự đa dạng về các cặp tiền
        var recentRates = await _repository.GetAllAsync(null, null, null, null, 0, 100, ct);
        if (recentRates == null || recentRates.Count == 0) return null;

        var random = new Random();
        // Chọn ngẫu nhiên một cặp tiền tệ từ danh sách gần đây
        var seed = recentRates[random.Next(recentRates.Count)];

        // Tự tính tỷ giá mới bằng cách biến động (+/- 5.0% để thay đổi rõ rệt theo yêu cầu)
        // Dải biến động: (random(0, 0.1) - 0.05) => +/- 0.05
        double fluctuation = (random.NextDouble() * 0.10) - 0.05;
        decimal newRate = seed.Rate * (decimal)(1 + fluctuation);

        // Đảm bảo tỷ giá hợp lý (không âm) và làm tròn đúng 4 chữ số thập phân
        newRate = Math.Max(0.0001m, Math.Round(newRate, 4));

        return new ExchangeRate
        {
            BaseCurrency = seed.BaseCurrency,
            TargetCurrency = seed.TargetCurrency,
            Rate = newRate,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.Now
        };
    }

    public async Task FetchAndSaveRangeRatesAsync(DateTime startDate, DateTime? endDate, string? baseCurrency = null, CancellationToken ct = default)
    {
        var endDisplay = endDate?.ToShortDateString() ?? "..";
        _logger.LogInformation("Fetching range rates from {Start} to {End} with base {Base}...", startDate.ToShortDateString(), endDisplay, baseCurrency ?? "default");
        var response = await _apiService.GetRangeRatesAsync(startDate, endDate, baseCurrency, ct);
        if (response is null || response.Rates is null || response.Rates.Count == 0)
        {
            _logger.LogWarning("No range rates found.");
            return;
        }

        var entities = new List<ExchangeRate>();
        foreach (var dailyRates in response.Rates)
        {
            if (!DateTime.TryParse(dailyRates.Key, out var timestamp)) continue;

            foreach (var targetRate in dailyRates.Value)
            {
                entities.Add(new ExchangeRate
                {
                    BaseCurrency = response.Base,
                    TargetCurrency = targetRate.Key,
                    Rate = targetRate.Value,
                    Timestamp = timestamp,
                    CreatedAt = timestamp
                });
            }
        }

        if (entities.Count > 0)
        {
            _logger.LogInformation("Saving {Count} range rates to database using bulk insert...", entities.Count);
            await _repository.AddRangeAsync(entities, ct);
        }
    }

    public Task<List<WorkerLog>> GetWorkerLogsAsync(int count, CancellationToken ct = default)
    {
        return _repository.GetWorkerLogsAsync(count, ct);
    }

    /// <summary>
    /// Kiểm tra ngày hôm nay đã có dữ liệu từ API chưa.
    /// Nếu chưa → gọi API lấy tỷ giá hôm nay và lưu vào DB.
    /// Trả về true nếu đã lưu mới, false nếu đã có sẵn.
    /// </summary>
    public async Task<bool> SyncTodayFromApiIfNeededAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var hasData = await _repository.HasDataForDateAsync(today, ct);

        if (hasData)
        {
            _logger.LogDebug("Today ({Date}) already has exchange rate data. Skipping API sync.", today.ToShortDateString());
            return false;
        }

        _logger.LogInformation("No data found for today ({Date}). Fetching from API...", today.ToShortDateString());
        await FetchAndSaveRangeRatesAsync(today, today, null, ct);
        return true;
    }
}

