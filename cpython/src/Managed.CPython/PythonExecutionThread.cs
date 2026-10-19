using System.Runtime.ExceptionServices;

namespace Managed.CPython;

// One caller, one outstanding request, one callback slot. This is a synchronous
// ownership boundary, not a general scheduler or SynchronizationContext pump.
internal sealed class PythonExecutionThread : IDisposable
{
    internal const int StackReservationBytes = 64 * 1024 * 1024;
    private readonly object gate = new();
    private readonly int caller = Environment.CurrentManagedThreadId;
    private readonly Thread thread;
    private Request? request;
    private CallbackRequest? callback;
    private ExceptionDispatchInfo? terminalFailure;
    private bool started, stop, exited;

    private abstract class Request
    {
        internal bool Completed;
        internal ExceptionDispatchInfo? Failure;
        internal abstract void Execute();
    }
    private sealed class Request<T>(Func<T> body) : Request
    {
        internal T Result = default!;
        internal override void Execute()
        {
            try { Result = body(); }
            catch (Exception error) { Failure = ExceptionDispatchInfo.Capture(error); }
        }
    }
    private sealed class CallbackRequest(Func<long, long> body, long argument)
    {
        internal readonly Func<long, long> Body = body;
        internal readonly long Argument = argument;
        internal long Result;
        internal bool Completed;
        internal ExceptionDispatchInfo? Failure;
    }

    internal PythonExecutionThread(Func<IDisposable>? enterContext = null)
    {
        thread = new Thread(() => Run(enterContext), StackReservationBytes)
        { IsBackground = true, Name = "CPython execution" };
        if (ExecutionContext.IsFlowSuppressed()) thread.Start();
        else { using (ExecutionContext.SuppressFlow()) thread.Start(); }
        lock (gate)
        {
            while (!started && !exited) Wait();
            if (terminalFailure != null) while (!exited) Wait();
            terminalFailure?.Throw();
        }
    }

    internal bool IsExecutionThread => ReferenceEquals(Thread.CurrentThread, thread);
    internal bool IsFaulted { get { lock (gate) return terminalFailure != null; } }

    private void RequireCaller()
    {
        if (caller != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("Only the initializing caller may submit CPython work.");
    }

    internal T Invoke<T>(Func<T> body)
    {
        RequireCaller();
        var pending = new Request<T>(body);
        lock (gate)
        {
            terminalFailure?.Throw();
            ObjectDisposedException.ThrowIf(stop || exited, this);
            if (request != null) throw new InvalidOperationException("A CPython operation is already outstanding.");
            request = pending;
            Monitor.PulseAll(gate);
        }
        // Waits are deliberately not cancellable: the interpreter owns request
        // arguments until completion. Cancellation uses the existing atomic C
        // bridge and trace-hook restoration, never abandonment of this wait.
        while (true)
        {
            CallbackRequest? work;
            lock (gate)
            {
                if (pending.Completed) { request = null; Monitor.PulseAll(gate); break; }
                work = callback;
                if (work == null) { Wait(); continue; }
            }
            try { work.Result = work.Body(work.Argument); }
            catch (Exception error) { work.Failure = ExceptionDispatchInfo.Capture(error); }
            finally
            {
                lock (gate)
                {
                    callback = null;
                    work.Completed = true;
                    Monitor.PulseAll(gate);
                }
            }
        }
        pending.Failure?.Throw();
        return pending.Result;
    }

    internal void Invoke(Action body) => Invoke(() => { body(); return 0; });

    // Genuine Python workers retain their invoking-thread callback affinity.
    // Only the executor can ask the waiting external owner for this narrow work.
    internal long InvokeCallback(Func<long, long> body, long argument)
    {
        if (!IsExecutionThread) return body(argument);
        var pending = new CallbackRequest(body, argument);
        lock (gate)
        {
            if (request == null || request.Completed || callback != null || exited)
                throw new InvalidOperationException("No waiting CPython caller for a synchronous callback.");
            callback = pending;
            Monitor.PulseAll(gate);
            while (!pending.Completed) Wait();
        }
        pending.Failure?.Throw();
        return pending.Result;
    }

    private void Run(Func<IDisposable>? enterContext)
    {
        IDisposable? binding = null;
        try
        {
            binding = enterContext?.Invoke();
            lock (gate) { started = true; Monitor.PulseAll(gate); }
            while (true)
            {
                Request pending;
                lock (gate)
                {
                    while (!stop && (request == null || request.Completed)) Wait();
                    if (stop) return;
                    pending = request!;
                }
                pending.Execute();
                lock (gate) { pending.Completed = true; Monitor.PulseAll(gate); }
            }
        }
        catch (Exception error) { RecordTerminalFailure(error); }
        finally
        {
            try { binding?.Dispose(); }
            catch (Exception error) { RecordTerminalFailure(error); }
            lock (gate) { exited = true; Monitor.PulseAll(gate); }
        }
    }

    private void RecordTerminalFailure(Exception error)
    {
        lock (gate)
        {
            terminalFailure ??= ExceptionDispatchInfo.Capture(error);
            if (request is { Completed: false } pending)
            { pending.Failure = terminalFailure; pending.Completed = true; }
            Monitor.PulseAll(gate);
        }
    }

    // Thread.Interrupt must not strand interpreter work or callback arguments.
    // It is not a supported Python cancellation mechanism.
    private void Wait()
    {
        try { Monitor.Wait(gate); }
        catch (ThreadInterruptedException) { }
    }

    public void Dispose()
    {
        RequireCaller();
        lock (gate)
        {
            if (request != null) throw new InvalidOperationException("Cannot stop an outstanding CPython request.");
            stop = true;
            Monitor.PulseAll(gate);
        }
        while (true)
        {
            try { thread.Join(); break; }
            catch (ThreadInterruptedException) { }
        }
        terminalFailure?.Throw();
    }
}
