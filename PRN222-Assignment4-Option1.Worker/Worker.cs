using Microsoft.Extensions.DependencyInjection;
using PRN222_Assignment4_Option1.BusinessLogic.Services;

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

    /// <summary>
    /// Chạy Worker; hỗ trợ dừng bằng CancellationToken khi host shutdown (Ctrl+C, Stop)
    /// và cho phép UI dừng / chạy lại thông qua IWorkerControlService.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Exchange Rate Worker started. Fetching random rate every {Seconds}s (no default base/target)", _intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_workerControl.IsRunning)
                {
                    // Worker đang bị dừng từ UI → chờ 1s rồi kiểm tra lại token.
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _workerControl.UiToken);
                var token = linkedCts.Token;

                using var scope = _scopeFactory.CreateScope();
                var exchangeRateService = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();
                await exchangeRateService.FetchAndSaveRandomRateAsync(token);
                _logger.LogInformation("Fetched and saved random rate at {Time}", DateTimeOffset.Now);

                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), token);
            }
            catch (OperationCanceledException)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    // Host (ứng dụng) đang shutdown → dừng hẳn worker.
                    break;
                }

                // Bị hủy do UI (UiToken) → quay lại vòng lặp, sẽ rơi vào nhánh IsRunning = false.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching/saving exchange rate");
            }
        }

        _logger.LogInformation("Exchange Rate Worker stopped.");
    }
}

