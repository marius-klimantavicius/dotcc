#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Edg.Runtime;

// Open-file-description locks on Linux and macOS isolate each emulated process from native code and other
// assemblies in the same CLR process. A duplicate anchors the open-file
// description without reopening a pathname (which could have been renamed).
// Close of ANY descriptor belonging to this virtual owner still drops all its
// inode locks, as POSIX process locks require. Other virtual owners are untouched.
internal static unsafe partial class PosixOfdLocks
{
    private static readonly object Gate = new();
    private readonly record struct Key(object Owner, ulong Device, ulong Inode);
    private readonly record struct Range(long Start, long End, short Type);
    private sealed class Anchor(SafeFileHandle handle, int mode)
    {
        internal readonly SafeFileHandle Handle = handle;
        internal readonly int Mode = mode;
        internal List<Range> Ranges = new();
    }
    private static readonly Dictionary<Key, Anchor> Anchors = new();

    private static Key Identity(object owner, SafeFileHandle handle, string path)
    {
        PosixStat info = PosixPlatform.Fstat(handle, path);
        return new(owner, info.st_dev, info.st_ino);
    }
    private static Anchor Duplicate(SafeFileHandle handle)
    {
        int mode = PosixPlatform.GetStatusFlags(handle);
        var duplicate = PosixPlatform.Duplicate(handle);
        try { return new Anchor(duplicate, mode & 3); }
        catch { duplicate.Dispose(); throw; }
    }
    private static void Request(SafeFileHandle handle, ref PosixFlock record)
    {
        record.l_pid = 0;
        int error = PosixPlatform.NativeLock(handle, 37, ref record); // F_OFD_SETLK
        if (error != 0) throw PosixPlatform.Failure(error);
    }
    private static void Unlock(Anchor anchor)
    {
        var record = new PosixFlock { l_type = 2 };
        Request(anchor.Handle, ref record); // length 0: through EOF
    }
    private static bool Allows(Anchor anchor, short type) => type == 2 || (type == 0 ? anchor.Mode != 1 : anchor.Mode != 0);

    // A read-only anchor may already have shared locks when a later read-write
    // descriptor requests an exclusive lock. Acquire identical shared ranges on
    // the replacement first: there is never a gap in kernel protection.
    private static Anchor EnsureMode(Key key, Anchor old, SafeFileHandle caller, short type)
    {
        if (Allows(old, type)) return old;
        Anchor replacement = Duplicate(caller);
        try
        {
            if (!Allows(replacement, type)) throw PosixPlatform.Failure(9);
            if (old.Ranges.Count != 0 && (old.Mode != 0 || replacement.Mode != 2))
                throw PosixPlatform.Failure(95);
            replacement.Ranges = new List<Range>(old.Ranges);
            foreach (Range held in old.Ranges)
            {
                var record = new PosixFlock { l_type = held.Type, l_start = held.Start,
                    l_len = held.End == long.MaxValue ? 0 : held.End - held.Start };
                Request(replacement.Handle, ref record);
            }
            Unlock(old);
            Anchors[key] = replacement;
            old.Handle.Dispose();
            return replacement;
        }
        catch
        {
            // The caller's original handle also refers to the replacement open
            // description, so closing its duplicate alone would retain copied
            // locks. Explicitly remove them before discarding the duplicate.
            try { Unlock(replacement); } finally { replacement.Handle.Dispose(); }
            throw;
        }
    }

    internal static void Control(object owner, SafeFileHandle handle, string path,
        int command, ref PosixFlock record, Func<bool>? closed = null)
    {
        if (command is not (5 or 6 or 7) || record.l_type is not (0 or 1 or 2) || record.l_whence != 0)
            throw PosixPlatform.Failure(22);
        if (command == 5 && record.l_type == 2) throw PosixPlatform.Failure(22);
        long start = record.l_start, end;
        try
        {
            if (record.l_len < 0) { end = start; start = checked(start + record.l_len); }
            else end = record.l_len == 0 ? long.MaxValue : checked(start + record.l_len);
        }
        catch (OverflowException) { throw PosixPlatform.Failure(75); }
        if (start < 0 || end <= start) throw PosixPlatform.Failure(22);
        Key key = Identity(owner, handle, path);
        lock (Gate)
        {
            while (true)
            {
                if (handle.IsClosed || closed?.Invoke() == true) throw PosixPlatform.Failure(9);
                if (!Anchors.TryGetValue(key, out Anchor? anchor))
                {
                    Anchors.EnsureCapacity(checked(Anchors.Count + 1));
                    anchor = Duplicate(handle); Anchors.Add(key, anchor);
                }
                var request = new PosixFlock { l_type = record.l_type, l_start = start,
                    l_len = end == long.MaxValue ? 0 : end - start };
                if (command == 5)
                {
                    // Query using the same OFD as the owner's held locks, so
                    // F_GETLK ignores its own ranges even on another descriptor.
                    int queryError = PosixPlatform.NativeLock(anchor.Handle, 36, ref request);
                    if (queryError != 0) throw PosixPlatform.Failure(queryError);
                    record = request; return;
                }
                anchor = EnsureMode(key, anchor, handle, record.l_type);
                List<Range> next = Replace(anchor.Ranges, start, end, record.l_type);
                // One kernel operation performs the entire conversion atomically.
                // A failed upgrade cannot weaken any existing portion of a lock.
                int error = PosixPlatform.NativeLock(anchor.Handle, 37, ref request);
                if (error == 0)
                {
                    anchor.Ranges = next; Monitor.PulseAll(Gate); return;
                }
                if (command == 7 && error is 11 or 13) { Monitor.Wait(Gate, 25); continue; }
                throw PosixPlatform.Failure(error);
            }
        }
    }
    private static List<Range> Replace(List<Range> old, long start, long end, short type)
    {
        var next = new List<Range>(old.Count + 2);
        foreach (Range held in old)
        {
            if (held.End <= start || held.Start >= end) { next.Add(held); continue; }
            if (held.Start < start) next.Add(held with { End = start });
            if (held.End > end) next.Add(held with { Start = end });
        }
        if (type != 2) next.Add(new(start, end, type));
        next.Sort((a, b) => a.Start.CompareTo(b.Start));
        for (int i = 0; i + 1 < next.Count;)
        {
            if (next[i].Type == next[i + 1].Type && next[i].End == next[i + 1].Start)
            { next[i] = next[i] with { End = next[i + 1].End }; next.RemoveAt(i + 1); }
            else i++;
        }
        return next;
    }
    internal static void Close(object owner, SafeFileHandle handle, string path)
    {
        lock (Gate)
        {
            try
            {
                Key key = Identity(owner, handle, path);
                if (Anchors.TryGetValue(key, out Anchor? anchor))
                {
                    Unlock(anchor); Anchors.Remove(key); anchor.Handle.Dispose();
                }
            }
            finally { handle.Dispose(); Monitor.PulseAll(Gate); }
        }
    }

}
