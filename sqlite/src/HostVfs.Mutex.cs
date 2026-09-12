#nullable enable
global using static Managed.Database.HostMutex;
using static global::Managed.Database.Sqlite;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Managed.Database;

/// <summary>SQLite APPDEF mutexes backed by BCL monitors. SQLite owns startup
/// ordering; this adapter does not initialize SQLite or replace its public API.</summary>
public static unsafe class HostMutex
{
    private const int Ok = 0, Busy = 5, NoMemory = 7;
    private const int FirstStatic = 2, LastStatic = 13;
    private static readonly object InitializationGate = new();
    private static nint[]? staticHandles;
    private static int dynamicCount;

    private sealed class MutexState
    {
        internal readonly object Gate = new();
        internal readonly bool IsStatic;
        internal MutexState(bool isStatic) => IsStatic = isStatic;
    }

    // A blittable static table has a stable address for the assembly's lifetime.
    // Each method address is captured exactly once, including assertion callbacks.
    private static readonly sqlite3_mutex_methods Methods = new()
    {
        xMutexInit = &Initialize, xMutexEnd = &End,
        xMutexAlloc = &Allocate, xMutexFree = &Free,
        xMutexEnter = &Enter, xMutexTry = &Try, xMutexLeave = &Leave,
        xMutexHeld = &Held, xMutexNotheld = &NotHeld
    };

    public static int DynamicMutexCount => Volatile.Read(ref dynamicCount);

    public static sqlite3_mutex_methods* dotcc_host_mutex_methods()
        => (sqlite3_mutex_methods*)Unsafe.AsPointer(ref Unsafe.AsRef(in Methods));

    public static void dotcc_host_memory_barrier() => Thread.MemoryBarrier();

    private static int Initialize()
    {
        lock (InitializationGate)
        {
            if (staticHandles != null) return Ok;
            nint[]? handles = null;
            try
            {
                handles = new nint[LastStatic - FirstStatic + 1];
                for (int index = 0; index < handles.Length; index++)
                    handles[index] = GCHandle.ToIntPtr(GCHandle.Alloc(new MutexState(isStatic: true)));
                // Publish only the complete set. Concurrent first callers all
                // pass through the gate; xMutexAlloc never sees partial startup.
                staticHandles = handles;
                return Ok;
            }
            catch (OutOfMemoryException)
            {
                if (handles != null)
                    foreach (nint handle in handles)
                        if (handle != 0) GCHandle.FromIntPtr(handle).Free();
                return NoMemory;
            }
        }
    }

    private static int End()
    {
        // These twelve process-lifetime static mutex identities mirror native
        // SQLite's static mutex array. No per-initialization resources remain;
        // connection and initialization mutexes are freed by SQLite itself.
        return Ok;
    }

    private static sqlite3_mutex* Allocate(int id)
    {
        if (id >= FirstStatic && id <= LastStatic)
        {
            if (Initialize() != Ok) return null;
            return (sqlite3_mutex*)staticHandles![id - FirstStatic];
        }
        if (id != 0 && id != 1) return null;
        try
        {
            nint handle = GCHandle.ToIntPtr(GCHandle.Alloc(new MutexState(isStatic: false)));
            Interlocked.Increment(ref dynamicCount);
            return (sqlite3_mutex*)handle;
        }
        catch (OutOfMemoryException) { return null; }
    }

    private static MutexState State(sqlite3_mutex* mutex)
        => (MutexState)GCHandle.FromIntPtr((nint)mutex).Target!;

    private static void Free(sqlite3_mutex* mutex)
    {
        if (mutex == null) return;
        var handle = GCHandle.FromIntPtr((nint)mutex);
        // Freeing a static or held mutex is caller misuse in SQLite. Avoid
        // invalidating static identities; dynamic lifetime is SQLite's contract.
        if (((MutexState)handle.Target!).IsStatic) return;
        handle.Free();
        Interlocked.Decrement(ref dynamicCount);
    }

    private static void Enter(sqlite3_mutex* mutex)
    {
        if (mutex != null) Monitor.Enter(State(mutex).Gate);
    }

    private static int Try(sqlite3_mutex* mutex)
        => mutex == null || Monitor.TryEnter(State(mutex).Gate) ? Ok : Busy;

    private static void Leave(sqlite3_mutex* mutex)
    {
        if (mutex != null) Monitor.Exit(State(mutex).Gate);
    }

    private static int Held(sqlite3_mutex* mutex)
        => mutex == null || Monitor.IsEntered(State(mutex).Gate) ? 1 : 0;

    private static int NotHeld(sqlite3_mutex* mutex)
        => mutex == null || !Monitor.IsEntered(State(mutex).Gate) ? 1 : 0;
}
