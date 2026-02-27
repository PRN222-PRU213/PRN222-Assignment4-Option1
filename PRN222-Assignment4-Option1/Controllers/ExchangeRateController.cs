using Microsoft.AspNetCore.Mvc;
using PRN222_Assignment4_Option1.BusinessLogic.Dtos;
using PRN222_Assignment4_Option1.BusinessLogic.Services;
using PRN222_Assignment4_Option1.Worker;

namespace PRN222_Assignment4_Option1.Controllers;

public class ExchangeRateController : Controller
{
    private const int PageSize = 20;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IWorkerControlService _workerControlService;
    private readonly IExchangeRateApiService _exchangeRateApiService;

    public ExchangeRateController(
        IExchangeRateService exchangeRateService,
        IWorkerControlService workerControlService,
        IExchangeRateApiService exchangeRateApiService)
    {
        _exchangeRateService = exchangeRateService;
        _workerControlService = workerControlService;
        _exchangeRateApiService = exchangeRateApiService;
    }

    public async Task<IActionResult> Index(
        DateTime? date,
        int page = 1,
        DateTime? apiDate = null,
        string? apiBase = null,
        string? apiSymbols = null,
        CancellationToken ct = default)
    {
        var skip = (page - 1) * PageSize;
        var list = await _exchangeRateService.GetListAsync(date, skip, PageSize + 1, ct);
        var hasNextPage = list.Count > PageSize;
        if (hasNextPage)
            list = list.Take(PageSize).ToList();

        ViewBag.DateFilter = date;
        ViewBag.Page = page;
        ViewBag.HasNextPage = hasNextPage;
        ViewBag.HasPrevPage = page > 1;
        ViewBag.IsWorkerRunning = _workerControlService.IsRunning;
        ViewBag.WorkerStatusMessage = TempData["WorkerStatusMessage"];

        // Nếu người dùng cung cấp bất kỳ giá trị filter API nào (ngày/base/symbols),
        // thì gọi Frankfurter API và hiển thị kết quả trên cùng trang.
        var hasApiFilter = apiDate.HasValue
                           || !string.IsNullOrWhiteSpace(apiBase)
                           || !string.IsNullOrWhiteSpace(apiSymbols);

        FrankfurterHistoricalResponse? apiResponse = null;
        DateTime? apiSelectedDate = null;

        if (hasApiFilter)
        {
            apiSelectedDate = apiDate ?? DateTime.Today;
            var baseCurNormalized = string.IsNullOrWhiteSpace(apiBase)
                ? null
                : apiBase.Trim().ToUpperInvariant();
            var symbolsNormalized = string.IsNullOrWhiteSpace(apiSymbols)
                ? null
                : apiSymbols.Trim().ToUpperInvariant();

            try
            {
                apiResponse = await _exchangeRateApiService.GetHistoricalRatesAsync(
                    apiSelectedDate.Value,
                    baseCurNormalized,
                    symbolsNormalized,
                    ct);
            }
            catch (OperationCanceledException)
            {
                // client hủy request → bỏ qua.
            }

            ViewBag.ApiHasFilter = true;
            ViewBag.ApiDate = apiSelectedDate;
            ViewBag.ApiBaseInput = apiBase;
            ViewBag.ApiSymbolsInput = apiSymbols;
            ViewBag.ApiResponse = apiResponse;
        }
        else
        {
            ViewBag.ApiHasFilter = false;
        }

        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult StopWorker()
    {
        _workerControlService.Stop();
        TempData["WorkerStatusMessage"] = "Worker đã được dừng từ UI.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult StartWorker()
    {
        _workerControlService.Start();
        TempData["WorkerStatusMessage"] = "Worker đã được khởi động lại từ UI.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Lọc tỷ giá trực tiếp từ Frankfurter API theo ngày, base và symbols
    /// mà không dùng database. Hỗ trợ ba trường hợp:
    /// - Chỉ ngày
    /// - Ngày + base
    /// - Ngày + base + symbols (1 hoặc nhiều mã, phân tách bởi dấu phẩy)
    /// </summary>
    public async Task<IActionResult> ApiFilter(DateTime? date, string? baseCurrency, string? symbols, CancellationToken ct = default)
    {
        var selectedDate = date ?? DateTime.Today;
        var baseCurNormalized = string.IsNullOrWhiteSpace(baseCurrency)
            ? null
            : baseCurrency.Trim().ToUpperInvariant();
        var symbolsNormalized = string.IsNullOrWhiteSpace(symbols)
            ? null
            : symbols.Trim().ToUpperInvariant();

        FrankfurterHistoricalResponse? response = null;
        try
        {
            response = await _exchangeRateApiService.GetHistoricalRatesAsync(
                selectedDate,
                baseCurNormalized,
                symbolsNormalized,
                ct);
        }
        catch (OperationCanceledException)
        {
            // Bị hủy bởi CancellationToken (client hủy request) → không coi là lỗi.
        }

        ViewBag.ApiDate = selectedDate;
        ViewBag.ApiBaseInput = baseCurrency;      // giữ nguyên để hiển thị lại trong ô nhập
        ViewBag.ApiSymbolsInput = symbols;
        ViewBag.ApiBase = baseCurNormalized;
        ViewBag.ApiSymbols = symbolsNormalized;
        ViewBag.ApiResponse = response;

        return View();
    }

    public async Task<IActionResult> Latest(CancellationToken ct = default)
    {
        var latest = await _exchangeRateService.GetLatestAsync(ct);
        return View(latest);
    }

    public async Task<IActionResult> Statistics(DateTime? date, CancellationToken ct = default)
    {
        var targetDate = date ?? DateTime.Today;
        var average = await _exchangeRateService.GetAverageRateByDateAsync(targetDate, null, null, ct);
        ViewBag.Date = targetDate;
        ViewBag.AverageRate = average;
        return View();
    }
}

