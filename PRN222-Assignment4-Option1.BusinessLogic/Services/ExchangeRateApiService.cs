using System.Net.Http.Json;
using PRN222_Assignment4_Option1.BusinessLogic.Dtos;

namespace PRN222_Assignment4_Option1.BusinessLogic.Services;

public interface IExchangeRateApiService
{
    Task<HexaRateData?> GetLatestRateAsync(string baseCurrency, string targetCurrency, CancellationToken ct = default);

    /// <summary>
    /// Lấy một tỷ giá ngẫu nhiên từ API (gọi /v1/latest không tham số, chọn ngẫu nhiên cặp base/target từ response).
    /// </summary>
    Task<HexaRateData?> GetRandomRateAsync(CancellationToken ct = default);

    /// <summary>
    /// Lấy toàn bộ tỷ giá mới nhất từ API (/v1/latest).
    /// </summary>
    Task<List<HexaRateData>> GetLatestRatesAsync(CancellationToken ct = default);

    /// <summary>
    /// Lấy tỷ giá lịch sử linh hoạt theo tài liệu Frankfurter:
    /// - Chỉ ngày:          /v1/{date}
    /// - Ngày + base:       /v1/{date}?base=USD
    /// - Ngày + base+symbols: /v1/{date}?base=USD&symbols=EUR,GBP
    /// Trả về toàn bộ rates trong ngày (có thể nhiều mã).
    /// </summary>
    Task<FrankfurterHistoricalResponse?> GetHistoricalRatesAsync(DateTime date, string? baseCurrency, string? symbols, CancellationToken ct = default);

    /// <summary>
    /// Lấy tỷ giá trong một khoảng thời gian: /v1/{start_date}..{end_date}?base={baseCurrency}
    /// </summary>
    Task<FrankfurterRangeResponse?> GetRangeRatesAsync(DateTime startDate, DateTime? endDate, string? baseCurrency, CancellationToken ct = default);
}

public sealed class ExchangeRateApiService : IExchangeRateApiService
{
    private readonly HttpClient _httpClient;

    public ExchangeRateApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HexaRateData?> GetLatestRateAsync(string baseCurrency, string targetCurrency, CancellationToken ct = default)
    {
        // Frankfurter: /v1/latest?base=USD&symbols=VND
        var url = $"v1/latest?base={baseCurrency}&symbols={targetCurrency}";

        var response = await _httpClient.GetFromJsonAsync<FrankfurterLatestResponse>(url, ct);
        if (response is null || response.Rates is null || !response.Rates.TryGetValue(targetCurrency, out var rate))
        {
            return null;
        }

        return new HexaRateData
        {
            Base = response.Base,
            Target = targetCurrency,
            Mid = rate,
            Unit = 1,
            Timestamp = response.Date
        };
    }

    public async Task<HexaRateData?> GetRandomRateAsync(CancellationToken ct = default)
    {
        // Frankfurter: GET /v1/latest không tham số → base=EUR, trả về tất cả rates. Chọn ngẫu nhiên 1 cặp.
        var response = await _httpClient.GetFromJsonAsync<FrankfurterLatestResponse>("v1/latest", ct);
        if (response is null || response.Rates is null || response.Rates.Count == 0)
            return null;

        var currencies = new List<string> { response.Base };
        currencies.AddRange(response.Rates.Keys);
        if (currencies.Count < 2)
            return null;

        var random = new Random();
        int baseIndex = random.Next(currencies.Count);
        int targetIndex;
        do
        {
            targetIndex = random.Next(currencies.Count);
        } while (targetIndex == baseIndex);

        string baseCurrency = currencies[baseIndex];
        string targetCurrency = currencies[targetIndex];

        decimal rate;
        if (baseCurrency == response.Base)
            rate = response.Rates[targetCurrency];
        else if (targetCurrency == response.Base)
            rate = 1m / response.Rates[baseCurrency];
        else
            rate = response.Rates[targetCurrency] / response.Rates[baseCurrency];

        return new HexaRateData
        {
            Base = baseCurrency,
            Target = targetCurrency,
            Mid = rate,
            Unit = 1,
            Timestamp = response.Date
        };
    }

    public async Task<List<HexaRateData>> GetLatestRatesAsync(CancellationToken ct = default)
    {
        // Frankfurter: /v1/latest -> mặc định base=EUR
        var response = await _httpClient.GetFromJsonAsync<FrankfurterLatestResponse>("v1/latest", ct);
        if (response is null || response.Rates is null)
        {
            return new List<HexaRateData>();
        }

        var result = new List<HexaRateData>();
        foreach (var kv in response.Rates)
        {
            result.Add(new HexaRateData
            {
                Base = response.Base,
                Target = kv.Key,
                Mid = kv.Value,
                Unit = 1,
                Timestamp = response.Date
            });
        }
        return result;
    }

    public async Task<FrankfurterHistoricalResponse?> GetHistoricalRatesAsync(DateTime date, string? baseCurrency, string? symbols, CancellationToken ct = default)
    {
        var dateString = date.ToString("yyyy-MM-dd");
        var path = $"v1/{dateString}";

        var queryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(baseCurrency))
        {
            queryParts.Add($"base={baseCurrency}");
        }
        if (!string.IsNullOrWhiteSpace(symbols))
        {
            queryParts.Add($"symbols={symbols}");
        }

        if (queryParts.Count > 0)
        {
            path += "?" + string.Join("&", queryParts);
        }

        using var httpResponse = await _httpClient.GetAsync(path, ct);
        if (!httpResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var response = await httpResponse.Content.ReadFromJsonAsync<FrankfurterHistoricalResponse>(cancellationToken: ct);
        if (response is null || response.Rates is null || response.Rates.Count == 0)
        {
            return null;
        }

        return response;
    }

    public async Task<FrankfurterRangeResponse?> GetRangeRatesAsync(DateTime startDate, DateTime? endDate, string? baseCurrency, CancellationToken ct = default)
    {
        var startStr = startDate.ToString("yyyy-MM-dd");
        var path = endDate.HasValue 
            ? $"v1/{startStr}..{endDate.Value.ToString("yyyy-MM-dd")}"
            : $"v1/{startStr}..";

        if (!string.IsNullOrWhiteSpace(baseCurrency))
        {
            path += $"?base={baseCurrency}";
        }

        using var httpResponse = await _httpClient.GetAsync(path, ct);
        if (!httpResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var response = await httpResponse.Content.ReadFromJsonAsync<FrankfurterRangeResponse>(cancellationToken: ct);
        return response;
    }
}

