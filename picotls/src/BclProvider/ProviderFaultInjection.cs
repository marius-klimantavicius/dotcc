using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ProviderVectors")]

namespace Managed.Security;

// Deterministic ownership failure tests. Production leaves current null; no
// public injection API, process-global state, or allocator replacement exists.
internal static class ProviderFaultInjection
{
    [ThreadStatic] private static FailureScope? current;
    internal static FailureScope FailAllocation(int ordinal) => new(ordinal);
    internal static void BeforeAllocation()
    {
        if (current is { } scope && ++scope.AllocationsAttempted == scope.Ordinal)
        {
            var error = new OutOfMemoryException("Injected provider allocation failure.");
            scope.InjectedFailure = error;
            throw error;
        }
    }
    internal static void StateDisposed()
    { if (current is { } scope) scope.DisposedStates++; }
    internal static void HashCloneFailed()
    { if (current is { } scope) scope.FailedHashClones++; }

    internal sealed class FailureScope : IDisposable
    {
        internal readonly int Ordinal;
        internal int AllocationsAttempted, DisposedStates, FailedHashClones;
        internal OutOfMemoryException? InjectedFailure;
        private readonly FailureScope? parent;
        private readonly int thread = Environment.CurrentManagedThreadId;
        private bool disposed;
        internal FailureScope(int ordinal)
        {
            if (ordinal <= 0) throw new ArgumentOutOfRangeException(nameof(ordinal));
            Ordinal = ordinal; parent = current; current = this;
        }
        public void Dispose()
        {
            if (disposed) return;
            if (thread != Environment.CurrentManagedThreadId || current != this)
                throw new InvalidOperationException("Provider fault scopes require stack-order disposal on their creating thread.");
            current = parent; disposed = true;
        }
    }
}
