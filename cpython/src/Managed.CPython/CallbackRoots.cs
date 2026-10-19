using System.Collections.Concurrent;

namespace Managed.CPython;

internal readonly record struct CallbackOutcome(bool Succeeded, long Value, string? Error);

internal static class CallbackRoots
{
    private sealed class Root(Func<long, long> callback, PythonExecutionThread? execution)
    {
        internal readonly Func<long, long> Callback = callback;
        internal readonly PythonExecutionThread? Execution = execution;
        internal int Active;
    }
    private static long nextToken;
    private static readonly ConcurrentDictionary<long, Root> roots = new();

    internal static long Add(Func<long, long> callback, PythonExecutionThread? execution = null)
    {
        ArgumentNullException.ThrowIfNull(callback);
        long token = Interlocked.Increment(ref nextToken);
        if (token <= 0) throw new InvalidOperationException("CPython callback tokens exhausted.");
        if (!roots.TryAdd(token, new Root(callback, execution))) throw new InvalidOperationException("Duplicate callback token.");
        return token;
    }

    // Called by a statically rooted, signature-correct translated METH_O thunk.
    // The GIL/drain contract prevents removal racing an entry. Registry access
    // does not hold a host monitor while calling application code.
    internal static CallbackOutcome Invoke(long token, long argument)
    {
        try
        {
            if (!roots.TryGetValue(token, out var root)) return new(false, 0, "Expired managed callback.");
            Interlocked.Increment(ref root.Active);
            try { return new(true, root.Execution is { } execution ? execution.InvokeCallback(root.Callback, argument) : root.Callback(argument), null); }
            catch (Exception error) { return new(false, 0, $"{error.GetType().Name}: {error.Message}"); }
            finally { Interlocked.Decrement(ref root.Active); }
        }
        catch (Exception) { return new(false, 0, "Managed callback failed before returning a result."); }
    }

    internal static void RemoveAfterDrain(long token)
    {
        if (!roots.TryGetValue(token, out var root)) throw new InvalidOperationException("Unknown callback token.");
        if (Volatile.Read(ref root.Active) != 0) throw new InvalidOperationException("Callback still executing during shutdown.");
        if (!roots.TryRemove(token, out _)) throw new InvalidOperationException("Callback already released.");
    }
}
