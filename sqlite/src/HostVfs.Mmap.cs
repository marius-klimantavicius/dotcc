#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace DotCC.Sqlite;

public static unsafe partial class HostVfs
{
    private const int IoMmap = IoError | (24 << 8);
    private static long mappedReads;
    private static int mappedViews, mappedReferences;
    public static long MappedReadCount => Interlocked.Read(ref mappedReads);
    public static int MappedViewCount => Volatile.Read(ref mappedViews);
    public static int MappedReferenceCount => Volatile.Read(ref mappedReferences);

    // Owns a read-only view of an existing descriptor. No extra file open/close:
    // on POSIX that could interfere with process-owned database byte locks.
    private sealed class DatabaseMapping(SafeFileHandle handle) : IDisposable
    {
        private readonly object gate = new();
        private readonly Dictionary<long, int> borrowed = new();
        private MemoryMappedFile? file;
        private MemoryMappedViewAccessor? view;
        private byte* address;
        private long length;
        private long limit = Math.Max(0, DotCcGlobals.sqlite3Config.szMmap);
        private int references;

        private void Unmap()
        {
            if (view == null) return;
            var previousView = view;
            var previousFile = file;
            view = null; file = null; address = null; length = 0;
            try
            {
                previousView.SafeMemoryMappedViewHandle.ReleasePointer();
                previousView.Dispose();
            }
            finally { previousFile!.Dispose(); Interlocked.Decrement(ref mappedViews); }
        }

        public int Configure(long* value)
        {
            lock (gate)
            {
                var requested = *value;
                *value = limit;
                // Match SQLite's native VFS: negative queries only; a live borrow
                // prevents a limit change that could invalidate its address.
                if (requested >= 0 && references == 0)
                {
                    requested = Math.Min(requested, Math.Max(0, DotCcGlobals.sqlite3Config.mxMmap));
                    if (requested != limit) { Unmap(); limit = requested; }
                }
                return Ok;
            }
        }

        private bool Map(long size)
        {
            MemoryMappedFile? nextFile = null;
            MemoryMappedViewAccessor? nextView = null;
            bool acquired = false;
            try
            {
                nextFile = MemoryMappedFile.CreateFromFile(handle, null, 0,
                    MemoryMappedFileAccess.Read, HandleInheritability.None, leaveOpen: true);
                nextView = nextFile.CreateViewAccessor(0, size, MemoryMappedFileAccess.Read);
                byte* pointer = null;
                nextView.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
                acquired = true;
                address = pointer + nextView.PointerOffset;
                file = nextFile; view = nextView; length = size;
                Interlocked.Increment(ref mappedViews);
                return true;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                or NotSupportedException or ArgumentException or OutOfMemoryException)
            {
                // Mapping is optional: SQLite will fall back to xRead when the
                // address is null, as its native VFS does for mmap failures.
                if (acquired) nextView!.SafeMemoryMappedViewHandle.ReleasePointer();
                try { nextView?.Dispose(); } finally { nextFile?.Dispose(); }
                Failure(exception, IoMmap);
                return false;
            }
        }

        public int Fetch(long offset, int count, void** output)
        {
            *output = null;
            if (count <= 0 || !ValidRange(count, offset)) return IoMmap;
            lock (gate)
            {
                // SQLite 3.53.4 reserves 256 addressable bytes past a mapped page
                // to tolerate limited overreads of a corrupt page. Never map the
                // final page without that buffer, even if the OS rounds the view.
                var end = offset + count;
                if (end > long.MaxValue - 256 || end + 256 > limit) return Ok;
                var required = end + 256;
                if (view == null || (required > length && references == 0))
                {
                    var size = Math.Min(RandomAccess.GetLength(handle), limit);
                    if (size < required) return Ok;
                    Unmap();
                    if (!Map(size)) return Ok;
                }
                if (required > length) return Ok;
                borrowed.TryGetValue(offset, out var existing);
                borrowed[offset] = checked(existing + 1);
                ++references;
                Interlocked.Increment(ref mappedReferences);
                Interlocked.Increment(ref mappedReads);
                *output = address + offset;
                return Ok;
            }
        }

        public int Unfetch(long offset, void* pointer)
        {
            lock (gate)
            {
                if (pointer == null)
                {
                    if (references != 0) return Busy;
                    Unmap();
                    return Ok;
                }
                if (offset < 0 || offset >= length || pointer != address + offset
                    || !borrowed.TryGetValue(offset, out var count)) return IoMmap;
                if (count == 1) borrowed.Remove(offset); else borrowed[offset] = count - 1;
                --references;
                Interlocked.Decrement(ref mappedReferences);
                // Windows prevents truncating files with mapped sections. Release
                // an idle view promptly so another connection/process can VACUUM
                // or checkpoint after the associated SQLite read lock is released.
                if (references == 0 && OperatingSystem.IsWindows()) Unmap();
                return Ok;
            }
        }

        public int BeforeTruncate()
        {
            lock (gate)
            {
                if (references != 0) return Busy;
                Unmap();
                return Ok;
            }
        }

        public void ReleaseIdle()
        {
            lock (gate) { if (references == 0) Unmap(); }
        }

        public void Dispose()
        {
            lock (gate)
            {
                // SQLite closes only after all page borrows have been released.
                // Still reclaim every VFS-owned resource on a raw caller's close.
                Interlocked.Add(ref mappedReferences, -references);
                references = 0; borrowed.Clear();
                Unmap();
            }
        }
    }

    private static int Fetch(sqlite3_file* file, long offset, int count, void** output)
    {
        *output = null;
        try { return State(file).Mapping?.Fetch(offset, count, output) ?? Ok; }
        catch (Exception exception) { return Failure(exception, IoMmap); }
    }

    private static int Unfetch(sqlite3_file* file, long offset, void* pointer)
    {
        try { return State(file).Mapping?.Unfetch(offset, pointer) ?? Ok; }
        catch (Exception exception) { return Failure(exception, IoMmap); }
    }
}
