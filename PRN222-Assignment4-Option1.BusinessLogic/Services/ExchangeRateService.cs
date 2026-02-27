using PRN222_Assignment4_Option1.BusinessLogic.Dtos;
using PRN222_Assignment4_Option1.DataAccess.Entities;
using PRN222_Assignment4_Option1.DataAccess.Repositories;

namespace PRN222_Assignment4_Option1.BusinessLogic.Services;

public sealed class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _repository;
    private readonly IExchangeRateApiService _apiService;

    public ExchangeRateService(IExchangeRateRepository repository, IExchangeRateApiService apiService)
    {
        _repository = repository;
        _apiService = apiService;
    }

    public Task<List<ExchangeRate>> GetListAsync(DateTime? date, int skip, int take, CancellationToken ct = default)
    {
        return _repository.GetAllAsync(date, skip, take, ct);
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
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, ct);
    }

    public async Task FetchAndSaveRandomRateAsync(CancellationToken ct = default)
    {
        var data = await _apiService.GetRandomRateAsync(ct);
        if (data is null)
            return;

        var entity = new ExchangeRate
        {
            BaseCurrency = data.Base,
            TargetCurrency = data.Target,
            Rate = data.Mid,
            Timestamp = data.Timestamp,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(entity, ct);
    }
}

