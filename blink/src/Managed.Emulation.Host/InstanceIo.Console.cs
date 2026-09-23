namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    // Borrowed from the run. Its lifetime extends past IO teardown so buffered
    // output remains readable after guest completion.
    private readonly HostConsole? console;
    private readonly CancellationTokenSource consoleLifetime = new();
    private int consoleOperations;
    public int PendingConsoleOperations { get { lock (sync) return consoleOperations; } }
    // Called with the descriptor gate held. Track the public completion before
    // starting work, and complete it before removing its drain reservation.
    private Task<HostResult<int>> RunConsole(Func<CancellationToken, Task<HostResult<int>>> operation, CancellationToken cancellation)
    {
        if (consoleOperations >= descriptorLimit) return Task.FromResult(Fail<int>(GuestError.Again));
        var completion = new TaskCompletionSource<HostResult<int>>(TaskCreationOptions.RunContinuationsAsynchronously);
        ++consoleOperations; pending.Add(completion.Task);
        _ = Execute();
        return completion.Task;
        async Task Execute()
        {
            HostResult<int> result = default;
            Exception? failure = null;
            try
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, consoleLifetime.Token);
                result = await operation(linked.Token).ConfigureAwait(false);
            }
            catch (Exception error) { failure = error; }
            lock (sync)
            {
                --consoleOperations;
                if (failure == null) completion.TrySetResult(result);
                else completion.TrySetException(failure);
                pending.Remove(completion.Task);
            }
        }
    }
    private void CloseConsoleDescription(Description description)
    {
        if (console == null) return;
        if (description.Kind == Kind.Input) console.CloseDescriptor(0);
        else if (description.Kind == Kind.Output) console.CloseDescriptor(1);
        else if (description.Kind == Kind.Error) console.CloseDescriptor(2);
    }
    private short ConsoleReadiness(Description description, short requested)
        => console!.Readiness(description.Kind == Kind.Input ? 0 : description.Kind == Kind.Output ? 1 : 2, requested);
}
