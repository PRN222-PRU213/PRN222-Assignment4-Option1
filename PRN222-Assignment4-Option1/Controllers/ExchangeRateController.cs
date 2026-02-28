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
    private readonly IConfiguration _configuration;

    public ExchangeRateController(
        IExchangeRateService exchangeRateService,
        IWorkerControlService workerControlService,
        IConfiguration configuration)
    {
        _exchangeRateService = exchangeRateService;
        _workerControlService = workerControlService;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(
        DateTime? startDate,
        DateTime? endDate,
        string? baseCurrency,
        string? symbols,
        int page = 1,
        CancellationToken ct = default)
    {
        var skip = (page - 1) * PageSize;
        var pagedResult = await _exchangeRateService.GetListAsync(startDate, endDate, baseCurrency, symbols, skip, PageSize, ct);
        
        var totalPages = (int)Math.Ceiling((double)pagedResult.TotalCount / PageSize);

        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.BaseCurrency = baseCurrency;
        ViewBag.Symbols = symbols;
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = pagedResult.TotalCount;
        ViewBag.IsWorkerRunning = _workerControlService.IsRunning;
        ViewBag.WorkerStatusMessage = TempData["WorkerStatusMessage"];

        return View(pagedResult.Items);
    }

    public async Task<IActionResult> Latest(CancellationToken ct = default)
    {
        var latest = await _exchangeRateService.GetLatestAsync(ct);
        return View(latest);
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

    public async Task<IActionResult> Logs(CancellationToken ct = default)
    {
        var logs = await _exchangeRateService.GetWorkerLogsAsync(100, ct);
        ViewBag.IsWorkerRunning = _workerControlService.IsRunning;
        ViewBag.IntervalSeconds = _configuration.GetValue("ExchangeRate:IntervalSeconds", 5);
        return View(logs);
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

