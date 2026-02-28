namespace ComputerClub.Services;

public interface ISessionTick
{
    void Tick();
}

public class SessionTickService : IDisposable
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly List<WeakReference<ISessionTick>> _targets = [];
    private CancellationTokenSource? _cts;

    public void Register(ISessionTick target)
    {
        _lock.Wait();
        try
        {
            _targets.Add(new WeakReference<ISessionTick>(target));
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Unregister(ISessionTick target)
    {
        _lock.Wait();
        try
        {
            _targets.RemoveAll(r =>
                r.TryGetTarget(out var t) && ReferenceEquals(t, target));
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Start()
    {
        if (_cts is not null) return;
        _cts = new CancellationTokenSource();
        _ = TickAsync(_cts.Token);
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                await _lock.WaitAsync(ct);
                try
                {
                    _targets.RemoveAll(r => !r.TryGetTarget(out _));

                    foreach (var reference in _targets)
                    {
                        if (reference.TryGetTarget(out var target))
                        {
                            target.Tick();
                        }
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}