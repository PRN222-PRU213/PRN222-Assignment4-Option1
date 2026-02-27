using System.Threading;

namespace PRN222_Assignment4_Option1.Worker;

/// <summary>
/// Cho phép UI điều khiển trạng thái chạy/dừng của Worker thông qua CancellationToken.
/// </summary>
public interface IWorkerControlService
{
    /// <summary>
    /// Token do UI điều khiển (Stop/Start). Được link với token của host bên trong Worker.
    /// </summary>
    CancellationToken UiToken { get; }

    /// <summary>
    /// Worker hiện có đang chạy hay đã dừng bởi UI.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Yêu cầu dừng Worker từ UI.
    /// </summary>
    void Stop();

    /// <summary>
    /// Cho phép Worker chạy lại sau khi đã dừng.
    /// </summary>
    void Start();
}

public sealed class WorkerControlService : IWorkerControlService
{
    private CancellationTokenSource _cts = new();
    private bool _isRunning = true;

    public CancellationToken UiToken => _cts.Token;

    public bool IsRunning => _isRunning;

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    public void Start()
    {
        if (_isRunning)
        {
            return;
        }

        _cts = new CancellationTokenSource();
        _isRunning = true;
    }
}

