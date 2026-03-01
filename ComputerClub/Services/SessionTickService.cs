using Microsoft.Extensions.DependencyInjection;

namespace ComputerClub.Services;

public interface ISessionTick
{
    void Tick();
}

public class SessionTickService(IServiceScopeFactory scopeFactory) : IDisposable
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

    private async Task TickAsync(CancellationToken ctx)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            var lastActivation = DateTime.MinValue;

            while (await timer.WaitForNextTickAsync(ctx))
            {
                await _lock.WaitAsync(ctx);
                try
                {
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

                if ((DateTime.UtcNow - lastActivation).TotalSeconds >= 30)
                {
                    lastActivation = DateTime.UtcNow;
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var service = scope.ServiceProvider.GetRequiredService<SessionService>();
                    await service.ActivateReservations(ctx);
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