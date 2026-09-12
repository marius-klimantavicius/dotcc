namespace Managed.Transport.Api;

internal sealed class QuicBudgetLease(Action release) : IDisposable
{
    private Action? release = release;
    public void Dispose() => Interlocked.Exchange(ref release, null)?.Invoke();
}
