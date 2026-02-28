using Microsoft.Extensions.DependencyInjection;
using PRN222_Assignment4_Option1.BusinessLogic.Services;
using PRN222_Assignment4_Option1.DataAccess.Data;
using PRN222_Assignment4_Option1.DataAccess.Entities;

namespace PRN222_Assignment4_Option1.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly int _intervalSeconds;
    private readonly IWorkerControlService _workerControl;

    public Worker(
        ILogger<Worker> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IWorkerControlService workerControl)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _intervalSeconds = configuration.GetValue("ExchangeRate:IntervalSeconds", 5);
        _workerControl = workerControl;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Exchange Rate Worker started. Interval: {Seconds}s", _intervalSeconds);
        await LogToDbAsync("Information", $"Worker started. Interval: {_intervalSeconds}s");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_workerControl.IsRunning)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _workerControl.UiToken);
                var token = linkedCts.Token;

                using var scope = _scopeFactory.CreateScope();
                var exchangeRateService = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();

                // Bước 1: Kiểm tra và đồng bộ tỷ giá từ API nếu hôm nay chưa có dữ liệu
                var synced = await exchangeRateService.SyncTodayFromApiIfNeededAsync(token);
                if (synced)
                {
                    _logger.LogInformation("Synced today's rates from real API.");
                    await LogToDbAsync("Information", "Đồng bộ tỷ giá hôm nay từ API thành công.");
                }

                // Bước 2: Luôn tạo thêm 1 tỷ giá mô phỏng (local simulation)
                var entity = await exchangeRateService.FetchAndSaveRandomRateAsync(token);
                if (entity != null)
                {
                    _logger.LogInformation("Fetched and saved random rate: {Base}->{Target} at {Rate}", entity.BaseCurrency, entity.TargetCurrency, entity.Rate);
                    await LogToDbAsync("Information", $"Tạo tỷ giá mới: {entity.BaseCurrency} -> {entity.TargetCurrency} = {entity.Rate:N4}");
                }

                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), token);
            }
            catch (OperationCanceledException)
            {
                if (stoppingToken.IsCancellationRequested) break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching/saving exchange rate");
                await LogToDbAsync("Error", $"Error: {ex.Message}", ex.ToString());
            }
        }

        _logger.LogInformation("Exchange Rate Worker stopped.");
        await LogToDbAsync("Information", "Worker stopped.");
    }

    private async Task LogToDbAsync(string level, string message, string? exception = null)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var log = new WorkerLog
            {
                Timestamp = DateTime.Now,
                LogLevel = level,
                Message = message,
                Exception = exception
            };
            context.WorkerLogs.Add(log);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log to database");
        }
    }
}

