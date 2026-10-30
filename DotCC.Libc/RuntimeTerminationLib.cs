#nullable enable
using System;
using System.Threading;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public enum RuntimeTerminationKind { Exit, ImmediateExit, Abort, WorkerFault }

    /// <summary>The first terminal request or worker fault for one C program.
    /// Status is the requested exit code for Exit/ImmediateExit; other kinds use
    /// zero. Fault retains the original managed exception for WorkerFault.</summary>
    public sealed record RuntimeTermination(RuntimeTerminationKind Kind, int Status, int ManagedThreadId, Exception? Fault = null);

    /// <summary>A nonreturning C termination request crossing an owning managed
    /// entry boundary. Catch only for the matching owner; stop dispatch and join
    /// its workers before disposal. Other program instances remain independent.</summary>
    public sealed class RuntimeTerminationException : Exception
    {
        public RuntimeContext Owner { get; }
        public RuntimeTermination Termination { get; }
        internal RuntimeTerminationException(RuntimeContext owner, RuntimeTermination termination)
            : base($"C program requested {termination.Kind} ({termination.Status}).", termination.Fault)
        { Owner = owner; Termination = termination; }
    }

    /// <summary>An explicitly intercepted exit returning to the embedding host.
    /// It neither records a terminal request nor performs guest cleanup.</summary>
    public sealed class RuntimeYieldException : Exception
    {
        public RuntimeContext Owner { get; }
        public int Status { get; }
        internal RuntimeYieldException(RuntimeContext owner, int status)
            : base($"C program yielded ({status}).") { Owner = owner; Status = status; }
    }

    [ThreadStatic] private static RuntimeExitScope? runtimeExitScope;

    /// <summary>Thread- and binding-scoped interception of host-selected exit
    /// statuses. Other exit statuses and immediate exits remain terminal.</summary>
    public sealed class RuntimeExitScope : IDisposable
    {
        internal readonly RuntimeContext Owner;
        internal readonly long Binding;
        private readonly int[] statuses;
        private readonly RuntimeExitScope? previous;
        private readonly int thread = Environment.CurrentManagedThreadId;
        private bool disposed;
        internal RuntimeExitScope(RuntimeContext owner, int[] statuses)
        {
            this.statuses = (int[])statuses.Clone();
            Owner = owner; Binding = currentRuntimeBinding;
            previous = runtimeExitScope; runtimeExitScope = this;
        }
        internal bool Intercepts(int status) => Array.IndexOf(statuses, status) >= 0;
        public void Dispose()
        {
            if (disposed) return;
            if (thread != Environment.CurrentManagedThreadId || runtimeExitScope != this ||
                RuntimeContext.Current != Owner || currentRuntimeBinding != Binding)
                throw new InvalidOperationException("Exit scopes must leave their original binding in stack order.");
            runtimeExitScope = previous; disposed = true;
        }
    }

    public sealed partial class RuntimeContext
    {
        private RuntimeTermination? termination;
        private readonly global::System.Threading.Tasks.TaskCompletionSource<RuntimeTermination> terminationCompletion =
            new(global::System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Terminal status, or null while no terminal request is known.
        /// This does not forcibly interrupt other workers or free their storage.</summary>
        public RuntimeTermination? Termination => Volatile.Read(ref termination);

        /// <summary>Asynchronous notification for the owning executor. Continuations
        /// never run synchronously in a translated worker's termination boundary.</summary>
        public global::System.Threading.Tasks.Task<RuntimeTermination> TerminationTask => terminationCompletion.Task;

        /// <summary>Temporarily turn the selected exit statuses into host yields.
        /// The caller owns the status policy; no status is special by default.</summary>
        public RuntimeExitScope InterceptExits(params int[] statuses)
        {
            ArgumentNullException.ThrowIfNull(statuses);
            if (RuntimeContext.Current != this || Termination != null)
                throw new InvalidOperationException("A live, explicitly bound owner is required.");
            return new RuntimeExitScope(this, statuses);
        }

        internal RuntimeTermination RecordTermination(RuntimeTerminationKind kind, int status, Exception? fault = null)
        {
            var requested = new RuntimeTermination(kind, status, Environment.CurrentManagedThreadId, fault);
            var selected = Interlocked.CompareExchange(ref termination, requested, null) ?? requested;
            StopGuestSignals();
            terminationCompletion.TrySetResult(selected);
            return selected;
        }

        internal void RecordWorkerFailure(Exception error)
        {
            // The explicit exit/abort path already recorded its owner. A fault
            // belonging to another owner is also a worker failure here: this
            // worker cannot safely resume the C call it was executing.
            if (error is RuntimeTerminationException requested && ReferenceEquals(requested.Owner, this)) return;
            RecordTermination(RuntimeTerminationKind.WorkerFault, 0, error);
        }
    }

    [global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
    private static void TerminateRuntime(RuntimeTerminationKind kind, int status)
    {
        if (RuntimeContext.Current is { } owner)
        {
            if (kind == RuntimeTerminationKind.Exit &&
                owner.Termination == null && runtimeExitScope is { } scope &&
                scope.Owner == owner && scope.Binding == currentRuntimeBinding && scope.Intercepts(status))
                throw new RuntimeYieldException(owner, status);
            throw new RuntimeTerminationException(owner, owner.RecordTermination(kind, status));
        }
        // Preserve standalone C program behavior. Only explicitly bound owners
        // opt into embedding; an ambient legacy runtime is not an embedding host.
        if (kind == RuntimeTerminationKind.Abort) Environment.FailFast("abort() called");
        Environment.Exit(status);
        throw new InvalidOperationException("Process termination returned unexpectedly.");
    }
}
