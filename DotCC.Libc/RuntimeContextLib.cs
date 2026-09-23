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
        public static RuntimeContext? Current => currentRuntimeBinding?.Context;
        internal RuntimeThreadState ThreadState => threadStates.GetValue(Thread.CurrentThread, static _ => new RuntimeThreadState());

        public IDisposable Enter()
        {
            Retain();
            try
            {
                var binding = new RuntimeBinding(this, currentRuntimeBinding);
                currentRuntimeBinding = binding;
                return binding;
            }
            catch { Release(); throw; }
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
    private sealed class RuntimeBinding(RuntimeContext context, RuntimeBinding? previous) : IDisposable
    {
        internal readonly RuntimeContext Context = context;
        private readonly int thread = Environment.CurrentManagedThreadId;
        private bool disposed;
        public void Dispose()
        {
            if (disposed) return;
            if (thread != Environment.CurrentManagedThreadId || !ReferenceEquals(currentRuntimeBinding, this))
                throw new InvalidOperationException("C program bindings must be left on their entering thread in stack order.");
            currentRuntimeBinding = previous;
            disposed = true;
            Context.Release();
        }
    }
    [ThreadStatic] private static RuntimeBinding? currentRuntimeBinding;
    private static readonly RuntimeContext legacyRuntime = new();
    private static RuntimeContext RuntimeState => RuntimeContext.Current ?? legacyRuntime;
    private static RuntimeThreadState RuntimeThread => RuntimeState.ThreadState;
}
