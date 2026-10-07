#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Edg.Runtime;

[StructLayout(LayoutKind.Sequential)]
public struct PosixSemaphoreTimespec
{
    public long tv_sec, tv_nsec;
}

public sealed unsafe partial class PosixContext
{
    private sealed class SemaphoreEntry
    {
        internal readonly object Gate = new();
        internal int Count, Waiters;
        internal bool Destroyed;
        internal SemaphoreEntry(int count) { Count = count; }
    }

    // Tokens are never reused, including between translated owner instances.
    private static long nextSemaphoreToken;
    private readonly object semaphoreGate = new();
    private readonly Dictionary<ulong, SemaphoreEntry> semaphores = new();
    private bool semaphoresClosed;

    private int SemaphoreError(int error) { SetError(error); return -1; }
    private static bool SemaphoreSlot(ulong* slot) => slot != null && ((nuint)slot & 7) == 0;

    private SemaphoreEntry? FindSemaphore(ulong* slot)
    {
        if (SemaphoreSlot(slot))
        {
            lock (semaphoreGate)
                if (!semaphoresClosed && semaphores.TryGetValue(*slot, out var entry)) return entry;
        }
        SetError(22);
        return null;
    }

    public int SemaphoreInit(ulong* slot, int processShared, uint value)
    {
        if (!SemaphoreSlot(slot) || value > int.MaxValue) return SemaphoreError(22);
        // Cross-process semaphores require shared OS storage. Never silently
        // substitute a process-local object for that distinct contract.
        if (processShared != 0) return SemaphoreError(38);
        try
        {
            var entry = new SemaphoreEntry((int)value);
            lock (semaphoreGate)
            {
                if (semaphoresClosed) return SemaphoreError(22);
                long fresh = Interlocked.Increment(ref nextSemaphoreToken);
                if (fresh <= 0) Environment.FailFast("Portable semaphore token space exhausted");
                semaphores.Add((ulong)fresh, entry);
                // sem_init accepts uninitialized storage; do not read its old
                // bytes as a token. Reinitializing an active sem_t is POSIX UB.
                *slot = (ulong)fresh;
            }
            return 0;
        }
        catch (OutOfMemoryException) { return SemaphoreError(12); }
    }

    public int SemaphoreDestroy(ulong* slot)
    {
        if (!SemaphoreSlot(slot)) return SemaphoreError(22);
        lock (semaphoreGate)
        {
            if (semaphoresClosed || !semaphores.TryGetValue(*slot, out var entry)) return SemaphoreError(22);
            lock (entry.Gate)
            {
                // POSIX leaves destruction while waiting undefined. Return a
                // useful error without losing a waiter or its semaphore token.
                if (entry.Waiters != 0) return SemaphoreError(16);
                entry.Destroyed = true;
                semaphores.Remove(*slot);
                *slot = 0;
                return 0;
            }
        }
    }

    public int SemaphorePost(ulong* slot)
    {
        var entry = FindSemaphore(slot);
        if (entry is null) return -1;
        lock (entry.Gate)
        {
            if (entry.Destroyed) return SemaphoreError(22);
            if (entry.Count == int.MaxValue) return SemaphoreError(75);
            ++entry.Count;
            if (entry.Waiters != 0) Monitor.Pulse(entry.Gate);
            return 0;
        }
    }

    public int SemaphoreGetValue(ulong* slot, int* value)
    {
        if (value == null) return SemaphoreError(22);
        var entry = FindSemaphore(slot);
        if (entry is null) return -1;
        lock (entry.Gate)
        {
            if (entry.Destroyed) return SemaphoreError(22);
            *value = entry.Count;
            return 0;
        }
    }

    public int SemaphoreWait(ulong* slot) => WaitSemaphore(slot, false, null);
    public int SemaphoreTryWait(ulong* slot) => WaitSemaphore(slot, true, null);
    public int SemaphoreTimedWait(ulong* slot, PosixSemaphoreTimespec* deadline)
    {
        if (deadline == null) return SemaphoreError(22);
        return WaitSemaphore(slot, false, deadline);
    }

    private int WaitSemaphore(ulong* slot, bool tryOnly, PosixSemaphoreTimespec* deadline)
    {
        var entry = FindSemaphore(slot);
        if (entry is null) return -1;
        lock (entry.Gate)
        {
            if (entry.Destroyed) return SemaphoreError(22);
            if (entry.Count != 0) { --entry.Count; return 0; }
            if (tryOnly) return SemaphoreError(11);
            // POSIX does not require validating the deadline if a token can
            // be acquired immediately, so this check follows the fast path.
            if (deadline != null && (deadline->tv_nsec < 0 || deadline->tv_nsec >= 1000000000))
                return SemaphoreError(22);
            Int128 absolute = deadline == null ? 0 : (Int128)deadline->tv_sec * 1000000000 + deadline->tv_nsec;
            ++entry.Waiters;
            try
            {
                while (entry.Count == 0)
                {
                    if (entry.Destroyed) return SemaphoreError(22);
                    int milliseconds = Timeout.Infinite;
                    if (deadline != null)
                    {
                        Int128 now = (Int128)(DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks) * 100;
                        Int128 remaining = absolute - now;
                        if (remaining <= 0) return SemaphoreError(110);
                        // Round upwards to avoid early timeout. Re-read UTC
                        // periodically because the POSIX deadline is realtime,
                        // not elapsed/monotonic time and may be adjusted.
                        Int128 rounded = (remaining + 999999) / 1000000;
                        milliseconds = rounded > 1000 ? 1000 : (int)rounded;
                    }
                    Monitor.Wait(entry.Gate, milliseconds);
                }
                if (entry.Destroyed) return SemaphoreError(22);
                --entry.Count;
                return 0;
            }
            catch (ThreadInterruptedException)
            {
                // A pending post must remain available to another waiter if
                // the selected thread was interrupted before consuming it.
                if (entry.Count != 0) Monitor.Pulse(entry.Gate);
                return SemaphoreError(4);
            }
            finally { --entry.Waiters; }
        }
    }

    private void CloseSemaphores()
    {
        lock (semaphoreGate)
        {
            semaphoresClosed = true;
            foreach (var entry in semaphores.Values)
            {
                lock (entry.Gate)
                {
                    entry.Destroyed = true;
                    Monitor.PulseAll(entry.Gate);
                }
            }
            semaphores.Clear();
        }
    }
}
