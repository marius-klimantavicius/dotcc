#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Explicit runtime identity carried by generated instance callback
    /// adapters. The caller must not substitute an unrelated ambient binding.</summary>
    public interface IProgramInstance
    {
        RuntimeContext __DotCcRuntime { get; }
    }

    /// <summary>Explicit runtime ownership for a generated C program context.
    /// Binding is thread-affine and stack ordered; it does not flow through async
    /// execution. Pinned globals and pthread objects belong to this context.
    /// Ambient filesystem, stdio and other host facilities still require the
    /// embedding application's explicit host bindings.</summary>
    public sealed class RuntimeContext : IDisposable
    {
        private readonly object gate = new();
        private int users;
        private bool disposed;
        internal readonly List<object> ArrayRoots = new();
        internal readonly HashSet<nuint> NativeAllocations = new();
        internal readonly List<(GCHandle Handle, Array Arr)> FunctionArrays = new();
        internal readonly ConcurrentDictionary<long, PthreadState> Threads = new();
        internal long NextThread;
        internal readonly object Objects = new();
        internal readonly Dictionary<int, PthreadMutex> Mutexes = new();
        internal readonly Dictionary<int, PthreadCondition> Conditions = new();
        internal int NextMutex, NextCondition;
        internal readonly ConcurrentDictionary<int, IntPtr> Keys = new();
        internal int NextKey;
        private readonly ConditionalWeakTable<Thread, RuntimeThreadState> threadStates = new();
        public object? ProgramState { get; }
        public RuntimeContext(object? programState = null) { ProgramState = programState; }
        public static RuntimeContext? Current => currentRuntimeContext;
        internal RuntimeThreadState ThreadState => threadStates.GetValue(Thread.CurrentThread, static _ => new RuntimeThreadState());

        public RuntimeBinding Enter()
        {
            // A unique token distinguishes recursive bindings of the same context
            // and prevents a copied/stale scope from releasing a newer binding.
            long token = checked(nextRuntimeBinding + 1);
            Retain();
            var binding = new RuntimeBinding(this, currentRuntimeContext, currentRuntimeBinding, token);
            nextRuntimeBinding = token;
            currentRuntimeContext = this;
            currentRuntimeBinding = token;
            return binding;
        }
        /// <summary>Reserves an instance for an explicitly registered deferred
        /// callback without binding this thread. The registration owner must
        /// dispose the lease after its last possible callback; disposal may
        /// occur on any thread and is idempotent.</summary>
        public IDisposable RetainLease()
        {
            Retain();
            try { return new RuntimeLease(this); }
            catch { Release(); throw; }
        }
        private sealed class RuntimeLease(RuntimeContext context) : IDisposable
        {
            private RuntimeContext? retained = context;
            public void Dispose() => Interlocked.Exchange(ref retained, null)?.Release();
        }
        internal void Retain()
        {
            lock (gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                users = checked(users + 1);
            }
        }
        internal void Release() { lock (gate) users--; }
        public void Dispose()
        {
            lock (gate)
            {
                if (disposed) return;
                if (users != 0) throw new InvalidOperationException("C program context still has active bindings or threads.");
                disposed = true;
                DisposeFileDescriptors(this);
                DisposeOwnedHeap(this);
                foreach (var item in FunctionArrays) item.Handle.Free();
                FunctionArrays.Clear(); ArrayRoots.Clear();
                Threads.Clear(); Mutexes.Clear(); Conditions.Clear(); Keys.Clear();
                threadStates.Clear();
            }
        }
    }
    internal sealed class RuntimeThreadState
    {
        internal int Errno, PthreadSets;
        internal long Self;
        internal bool Created;
        internal Dictionary<int, IntPtr>? Values;
    }
    /// <summary>Allocation-free, thread-affine runtime scope. Dispose in stack
    /// order and do not dispose multiple copies of the same active scope.</summary>
    public struct RuntimeBinding : IDisposable
    {
        private RuntimeContext? context;
        private readonly RuntimeContext? previousContext;
        private readonly long previousToken, token;
        private readonly int thread;

        internal RuntimeBinding(RuntimeContext context, RuntimeContext? previousContext,
            long previousToken, long token)
        {
            this.context = context;
            this.previousContext = previousContext;
            this.previousToken = previousToken;
            this.token = token;
            thread = Environment.CurrentManagedThreadId;
        }

        public void Dispose()
        {
            if (context is null) return;
            if (thread != Environment.CurrentManagedThreadId || currentRuntimeBinding != token)
                throw new InvalidOperationException("C program bindings must be left on their entering thread in stack order.");
            currentRuntimeContext = previousContext;
            currentRuntimeBinding = previousToken;
            var released = context;
            context = null;
            released.Release();
        }
    }
    [ThreadStatic] private static RuntimeContext? currentRuntimeContext;
    [ThreadStatic] private static long currentRuntimeBinding, nextRuntimeBinding;
    private static readonly RuntimeContext legacyRuntime = new();
    private static RuntimeContext RuntimeState => RuntimeContext.Current ?? legacyRuntime;
    private static RuntimeThreadState RuntimeThread => RuntimeState.ThreadState;
}
