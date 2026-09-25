#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if DOTCC_SQLITE_PRODUCT
using static Managed.Database.Sqlite;
#else
using static DotCcProgram;
#endif

namespace Managed.Database;

/// <summary>Process-local, deterministic SQLite VFS. No disk, WAL or mmap support.</summary>
internal static unsafe class MemoryVfs
{
    private const int PathBytes = 1025;
    private const int FailOpen = 1, FailRead = 2, FailWrite = 4, FailTruncate = 8, FailSync = 16, FailDelete = 32;
    private const long InitialTime = 1735689600000;
    // No SQLite allocator or mutex call while holding this gate. Initialization,
    // failure injection and cold restart therefore cannot invert SQLite's locks.
    private static readonly object Gate = new();
    private static readonly List<Node> Nodes = [];
    private static readonly Dictionary<nint, OpenFile> Handles = [];
    private static int _failMask, _failCount, _failResult;
    private static uint _random = 1;
    private static long _unixMilliseconds = InitialTime;
    private static sqlite3_vfs* _vfs;
    private static sqlite3_io_methods* _methods;

    private sealed class Node(string? name)
    {
        public readonly string? Name = name;
        public byte[] Data = [];
        public int Size, References;
        public bool Unlinked = name == null;
    }
    private sealed class OpenFile(Node node, int flags)
    {
        public readonly Node Node = node;
        public readonly int Flags = flags;
        public int LockLevel;
    }

    // Capture function addresses once; preserve their exact bits across GC/tiering.
    private static readonly sqlite3_io_methods Methods = new()
    {
        iVersion = 1, xClose = &Close, xRead = &Read, xWrite = &Write,
        xTruncate = &Truncate, xSync = &Sync, xFileSize = &Size,
        xLock = &LockFile, xUnlock = &Unlock, xCheckReservedLock = &Reserved,
        xFileControl = &Control, xSectorSize = &Sector, xDeviceCharacteristics = &Device,
    };
    private static readonly sqlite3_vfs Vfs = new()
    {
        iVersion = 1, szOsFile = sizeof(sqlite3_file), mxPathname = PathBytes - 1,
        xOpen = &Open, xDelete = &Delete, xAccess = &Access, xFullPathname = &FullPath,
        xRandomness = &Randomness, xSleep = &Sleep, xCurrentTime = &Time, xGetLastError = &Error,
    };

    public static sqlite3_vfs* GetVfs()
    {
        lock (Gate)
        {
            if (_vfs != null) return _vfs;
            sqlite3_io_methods* methods = null;
            sqlite3_vfs* vfs = null;
            byte* name = null;
            try
            {
                methods = (sqlite3_io_methods*)NativeMemory.Alloc((nuint)sizeof(sqlite3_io_methods));
                vfs = (sqlite3_vfs*)NativeMemory.Alloc((nuint)sizeof(sqlite3_vfs));
                name = (byte*)NativeMemory.Alloc(13);
                *methods = Methods;
                *vfs = Vfs;
                "dotcc-memory\0"u8.CopyTo(new Span<byte>(name, 13));
                vfs->zName = name;
                _methods = methods;
                _vfs = vfs;
                return vfs;
            }
            catch (OutOfMemoryException) { return null; }
            finally
            {
                if (_vfs == null)
                {
                    NativeMemory.Free(methods);
                    NativeMemory.Free(vfs);
                    NativeMemory.Free(name);
                }
            }
        }
    }

    public static int Initialize()
    {
        var vfs = GetVfs();
        if (vfs == null) return SQLITE_NOMEM;
#if DOTCC_SQLITE_PRODUCT
        int rc = sqlite3_vfs_register(vfs, 0);
        if (rc != SQLITE_OK) return rc;
        rc = HostVfs.RegisterVfs();
        if (rc != SQLITE_OK) sqlite3_vfs_unregister(vfs);
        return rc;
#else
        return sqlite3_vfs_register(vfs, 1);
#endif
    }

    public static int Shutdown()
    {
#if DOTCC_SQLITE_PRODUCT
        int rc = HostVfs.UnregisterVfs();
        if (rc != SQLITE_OK) return rc;
#endif
        // SQLite registrations may retain the canonical table addresses. Keep
        // the small tables for the process lifetime, as HostVfs does on restart.
        return _vfs == null ? SQLITE_OK : sqlite3_vfs_unregister(_vfs);
    }

    private static int Failure(int operation)
    {
        if ((_failMask & operation) == 0) return SQLITE_OK;
        if (_failCount > 0) { --_failCount; return SQLITE_OK; }
        _failMask = 0;
        return _failResult;
    }

    private static int Normalize(byte* name, Span<byte> output, out int length)
    {
        length = 0;
        if (name == null || output.Length < 2) return SQLITE_CANTOPEN;
        int used = 1;
        output[0] = (byte)'/';
        while (*name != 0)
        {
            while (*name == '/') ++name;
            byte* start = name;
            while (*name != 0 && *name != '/') ++name;
            long size = name - start;
            if (size == 0 || (size == 1 && start[0] == '.')) continue;
            if (size == 2 && start[0] == '.' && start[1] == '.')
            {
                while (used > 1 && output[used - 1] != '/') --used;
                if (used > 1) --used;
                continue;
            }
            if (size > PathBytes - 1 || used + size + (used > 1 ? 1 : 0) >= output.Length)
                return SQLITE_CANTOPEN;
            if (used > 1) output[used++] = (byte)'/';
            new ReadOnlySpan<byte>(start, (int)size).CopyTo(output[used..]);
            used += (int)size;
        }
        output[used] = 0;
        length = used;
        return SQLITE_OK;
    }

    private static int PathKey(byte* name, out string key)
    {
        Span<byte> path = stackalloc byte[PathBytes];
        int rc = Normalize(name, path, out int length);
        // Preserve arbitrary pathname bytes, including invalid UTF-8.
        key = rc == SQLITE_OK ? Convert.ToHexString(path[..length]) : "";
        return rc;
    }
    private static Node? Find(string name)
    {
        foreach (var node in Nodes)
            if (!node.Unlinked && node.Name == name) return node;
        return null;
    }
    private static int Resize(Node node, long size)
    {
        if (size < 0 || size > Array.MaxLength) return SQLITE_FULL;
        int length = (int)size;
        if (length > node.Data.Length)
        {
            int capacity = (int)Math.Min(((size + 4095) / 4096) * 4096, Array.MaxLength);
            Array.Resize(ref node.Data, capacity);
        }
        if (length > node.Size) Array.Clear(node.Data, node.Size, length - node.Size);
        node.Size = length;
        return SQLITE_OK;
    }

    private static int Open(sqlite3_vfs* vfs, byte* name, sqlite3_file* file, int flags, int* outputFlags)
    {
        file->pMethods = null;
        lock (Gate)
        {
            Node? created = null;
            try
            {
                int rc = Failure(FailOpen);
                if (rc != SQLITE_OK) return rc;
                if ((flags & (SQLITE_OPEN_READONLY | SQLITE_OPEN_READWRITE)) == 0 ||
                    (flags & (SQLITE_OPEN_READONLY | SQLITE_OPEN_READWRITE)) == (SQLITE_OPEN_READONLY | SQLITE_OPEN_READWRITE) ||
                    ((flags & SQLITE_OPEN_CREATE) != 0 && (flags & SQLITE_OPEN_READWRITE) == 0))
                    return SQLITE_CANTOPEN;
                Node? node;
                if (name != null)
                {
                    rc = PathKey(name, out string path);
                    if (rc != SQLITE_OK) return rc;
                    node = Find(path);
                    if (node != null && (flags & (SQLITE_OPEN_EXCLUSIVE | SQLITE_OPEN_CREATE)) == (SQLITE_OPEN_EXCLUSIVE | SQLITE_OPEN_CREATE))
                        return SQLITE_CANTOPEN;
                    if (node == null && (flags & SQLITE_OPEN_CREATE) == 0) return SQLITE_CANTOPEN;
                    if (node == null) node = created = new Node(path);
                }
                else
                {
                    if ((flags & SQLITE_OPEN_DELETEONCLOSE) == 0) return SQLITE_CANTOPEN;
                    node = created = new Node(null);
                }
                if (created != null) Nodes.Add(created);
                Handles.Add((nint)file, new OpenFile(node, flags));
                ++node.References;
                file->pMethods = _methods;
                if (outputFlags != null) *outputFlags = flags;
                return SQLITE_OK;
            }
            catch (OutOfMemoryException)
            {
                if (created != null) Nodes.Remove(created);
                return SQLITE_NOMEM;
            }
        }
    }

    private static int Close(sqlite3_file* file)
    {
        lock (Gate)
        {
            var state = Handles[(nint)file];
            Handles.Remove((nint)file);
            if ((state.Flags & SQLITE_OPEN_DELETEONCLOSE) != 0) state.Node.Unlinked = true;
            if (--state.Node.References == 0 && state.Node.Unlinked) Nodes.Remove(state.Node);
            file->pMethods = null;
            return SQLITE_OK;
        }
    }
    private static int Read(sqlite3_file* file, void* buffer, int count, long offset)
    {
        lock (Gate)
        {
            int rc = Failure(FailRead);
            if (rc != SQLITE_OK) return rc;
            if (offset < 0 || count < 0) return SQLITE_IOERR_READ;
            var node = Handles[(nint)file].Node;
            int available = (int)Math.Min(count, offset < node.Size ? node.Size - offset : 0);
            var target = new Span<byte>(buffer, count);
            if (available != 0) node.Data.AsSpan((int)offset, available).CopyTo(target);
            target[available..].Clear();
            return available < count ? SQLITE_IOERR_SHORT_READ : SQLITE_OK;
        }
    }
    private static int Write(sqlite3_file* file, void* buffer, int count, long offset)
    {
        lock (Gate)
        {
            try
            {
                var state = Handles[(nint)file];
                if ((state.Flags & SQLITE_OPEN_READONLY) != 0) return SQLITE_READONLY;
                int rc = Failure(FailWrite);
                if (rc != SQLITE_OK) return rc;
                if (offset < 0 || count < 0 || offset > long.MaxValue - count) return SQLITE_IOERR_WRITE;
                if (count == 0) return SQLITE_OK;
                if (offset + count > state.Node.Size)
                {
                    rc = Resize(state.Node, offset + count);
                    if (rc != SQLITE_OK) return rc;
                }
                new ReadOnlySpan<byte>(buffer, count).CopyTo(state.Node.Data.AsSpan((int)offset, count));
                return SQLITE_OK;
            }
            catch (OutOfMemoryException) { return SQLITE_NOMEM; }
        }
    }
    private static int Truncate(sqlite3_file* file, long size)
    {
        lock (Gate)
        {
            try
            {
                var state = Handles[(nint)file];
                if ((state.Flags & SQLITE_OPEN_READONLY) != 0) return SQLITE_READONLY;
                int rc = Failure(FailTruncate);
                return rc != SQLITE_OK ? rc : size < 0 ? SQLITE_IOERR_TRUNCATE : Resize(state.Node, size);
            }
            catch (OutOfMemoryException) { return SQLITE_NOMEM; }
        }
    }
    private static int Sync(sqlite3_file* file, int flags) { lock (Gate) return Failure(FailSync); }
    private static int Size(sqlite3_file* file, long* size)
    {
        lock (Gate) { *size = Handles[(nint)file].Node.Size; return SQLITE_OK; }
    }
    private static int LockFile(sqlite3_file* file, int level)
    {
        lock (Gate)
        {
            var state = Handles[(nint)file];
            if (level <= state.LockLevel) return SQLITE_OK;
            if (level < SQLITE_LOCK_SHARED || level > SQLITE_LOCK_EXCLUSIVE) return SQLITE_IOERR_LOCK;
            foreach (var other in Handles.Values)
            {
                if (other == state || other.Node != state.Node) continue;
                if ((level == SQLITE_LOCK_SHARED && other.LockLevel >= SQLITE_LOCK_PENDING) ||
                    (level >= SQLITE_LOCK_RESERVED && other.LockLevel >= SQLITE_LOCK_RESERVED)) return SQLITE_BUSY;
            }
            if (level == SQLITE_LOCK_EXCLUSIVE)
            {
                state.LockLevel = SQLITE_LOCK_PENDING;
                foreach (var other in Handles.Values)
                    if (other != state && other.Node == state.Node && other.LockLevel >= SQLITE_LOCK_SHARED)
                        return SQLITE_BUSY;
            }
            state.LockLevel = level;
            return SQLITE_OK;
        }
    }
    private static int Unlock(sqlite3_file* file, int level)
    {
        lock (Gate)
        {
            if (level != SQLITE_LOCK_NONE && level != SQLITE_LOCK_SHARED) return SQLITE_IOERR_UNLOCK;
            var state = Handles[(nint)file];
            if (level < state.LockLevel) state.LockLevel = level;
            return SQLITE_OK;
        }
    }
    private static int Reserved(sqlite3_file* file, int* result)
    {
        lock (Gate)
        {
            var node = Handles[(nint)file].Node;
            *result = 0;
            foreach (var other in Handles.Values)
                if (other.Node == node && other.LockLevel >= SQLITE_LOCK_RESERVED) *result = 1;
            return SQLITE_OK;
        }
    }
    private static int Control(sqlite3_file* file, int operation, void* argument)
    {
        lock (Gate)
        {
            if (operation != SQLITE_FCNTL_LOCKSTATE) return SQLITE_NOTFOUND;
            *(int*)argument = Handles[(nint)file].LockLevel;
            return SQLITE_OK;
        }
    }
    private static int Sector(sqlite3_file* file) => 512;
    private static int Device(sqlite3_file* file) => 0;

    private static int Delete(sqlite3_vfs* vfs, byte* name, int syncDirectory)
    {
        lock (Gate)
        {
            try
            {
                int rc = Failure(FailDelete);
                if (rc != SQLITE_OK) return rc;
                rc = PathKey(name, out string path);
                if (rc != SQLITE_OK) return rc;
                var node = Find(path);
                if (node == null) return SQLITE_IOERR_DELETE_NOENT;
                node.Unlinked = true;
                if (node.References == 0) Nodes.Remove(node);
                return SQLITE_OK;
            }
            catch (OutOfMemoryException) { return SQLITE_NOMEM; }
        }
    }
    private static int Access(sqlite3_vfs* vfs, byte* name, int flags, int* result)
    {
        lock (Gate)
        {
            *result = 0;
            try
            {
                if (flags != SQLITE_ACCESS_EXISTS && flags != SQLITE_ACCESS_READ && flags != SQLITE_ACCESS_READWRITE)
                    return SQLITE_IOERR_ACCESS;
                int rc = PathKey(name, out string path);
                if (rc == SQLITE_OK) *result = Find(path) == null ? 0 : 1;
                return rc;
            }
            catch (OutOfMemoryException) { return SQLITE_NOMEM; }
        }
    }
    private static int FullPath(sqlite3_vfs* vfs, byte* name, int count, byte* output) =>
        count < 2 ? SQLITE_CANTOPEN : Normalize(name, new Span<byte>(output, count), out _);
    private static int Randomness(sqlite3_vfs* vfs, int count, byte* output)
    {
        lock (Gate)
        {
            for (int i = 0; i < count; i++)
            {
                _random = unchecked(_random * 1664525U + 1013904223U);
                output[i] = (byte)(_random >> 24);
            }
            return count;
        }
    }
    private static int Sleep(sqlite3_vfs* vfs, int microseconds)
    {
        lock (Gate)
        {
            if (microseconds > 0) _unixMilliseconds = unchecked(_unixMilliseconds + microseconds / 1000);
            return Math.Max(0, microseconds);
        }
    }
    private static int Time(sqlite3_vfs* vfs, double* julianDay)
    {
        lock (Gate) { *julianDay = 2440587.5 + _unixMilliseconds / 86400000.0; return SQLITE_OK; }
    }
    private static int Error(sqlite3_vfs* vfs, int count, byte* output)
    {
        if (count > 0) output[0] = 0;
        return 0;
    }

    public static int Reset()
    {
        lock (Gate)
        {
            if (Handles.Count != 0) return SQLITE_BUSY;
            Nodes.Clear();
            _failMask = 0;
            _random = 1;
            _unixMilliseconds = InitialTime;
            return SQLITE_OK;
        }
    }
    public static int FileCount() { lock (Gate) return Nodes.Count; }
    public static int HandleCount() { lock (Gate) return Handles.Count; }
    public static void FailAfter(int mask, int successfulCalls, int result)
    {
        lock (Gate)
        {
            _failMask = mask;
            _failCount = Math.Max(0, successfulCalls);
            _failResult = result == SQLITE_OK ? SQLITE_IOERR : result;
        }
    }
    public static void Seed(uint seed) { lock (Gate) _random = seed; }
    public static void SetTime(long unixMilliseconds) { lock (Gate) _unixMilliseconds = unixMilliseconds; }
    public static int Export(byte* name, void* buffer, long capacity, long* size)
    {
        lock (Gate)
        {
            try
            {
                if (size == null || capacity < 0) return SQLITE_MISUSE;
                int rc = PathKey(name, out string path);
                if (rc != SQLITE_OK) return rc;
                var node = Find(path);
                if (node == null) return SQLITE_NOTFOUND;
                if (node.References != 0) return SQLITE_BUSY;
                *size = node.Size;
                if (buffer == null) return SQLITE_OK;
                if (capacity < node.Size) return SQLITE_TOOBIG;
                node.Data.AsSpan(0, node.Size).CopyTo(new Span<byte>(buffer, node.Size));
                return SQLITE_OK;
            }
            catch (OutOfMemoryException) { return SQLITE_NOMEM; }
        }
    }
    public static int Import(byte* name, void* buffer, long size)
    {
        lock (Gate)
        {
            Node? created = null;
            try
            {
                if (size < 0 || (buffer == null && size != 0)) return SQLITE_MISUSE;
                int rc = PathKey(name, out string path);
                if (rc != SQLITE_OK) return rc;
                var node = Find(path);
                if (node != null && node.References != 0) return SQLITE_BUSY;
                if (node == null) node = created = new Node(path);
                rc = Resize(node, size);
                if (rc != SQLITE_OK) return rc;
                new ReadOnlySpan<byte>(buffer, (int)size).CopyTo(node.Data);
                if (created != null) Nodes.Add(created);
                return SQLITE_OK;
            }
            catch (OutOfMemoryException) { return SQLITE_NOMEM; }
        }
    }
}
