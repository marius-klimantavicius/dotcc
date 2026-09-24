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

        internal RuntimeTermination RecordTermination(RuntimeTerminationKind kind, int status, Exception? fault = null)
        {
            var requested = new RuntimeTermination(kind, status, Environment.CurrentManagedThreadId, fault);
            var selected = Interlocked.CompareExchange(ref termination, requested, null) ?? requested;
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
            throw new RuntimeTerminationException(owner, owner.RecordTermination(kind, status));
        // Preserve standalone C program behavior. Only explicitly bound owners
        // opt into embedding; an ambient legacy runtime is not an embedding host.
        if (kind == RuntimeTerminationKind.Abort) Environment.FailFast("abort() called");
        Environment.Exit(status);
        throw new InvalidOperationException("Process termination returned unexpectedly.");
    }
}
