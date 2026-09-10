#nullable enable
global using static DotCC.Sqlite.HostVfs;

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using static DotCcLib;

namespace DotCC.Sqlite;

/// <summary>File-backed SQLite rollback-journal and WAL VFS. OS services only; no native SQLite.</summary>
public static unsafe class HostVfs
{
    private const int Ok = 0, Error = 1, Busy = 5, NoMemory = 7, ReadOnly = 8,
        IoError = 10, Full = 13, CannotOpen = 14, NotFound = 12;
    private const int OpenReadOnly = 1, OpenReadWrite = 2, OpenCreate = 4,
        DeleteOnClose = 8, Exclusive = 16, MainDb = 0x100, NoFollow = 0x1000000;
    private const int PathBytes = 32768;
    private static readonly object RegistrationGate = new();
    private static sqlite3_vfs* registeredVfs;
    private static sqlite3_io_methods* registeredMethods;
    private static int openHandles;
    [ThreadStatic] private static string? lastError;
    [ThreadStatic] private static int lastErrorNumber;

    // Each method address is captured once. Copies into unmanaged tables retain
    // identical pointer bits through tiering and collection.
    private static readonly sqlite3_io_methods MethodTable = new()
    {
        iVersion = 2, xClose = &Close, xRead = &Read, xWrite = &Write,
        xTruncate = &Truncate, xSync = &Sync, xFileSize = &FileSize,
        xLock = &Lock, xUnlock = &Unlock, xCheckReservedLock = &CheckReserved,
        xFileControl = &FileControl, xSectorSize = &SectorSize,
        xDeviceCharacteristics = &DeviceCharacteristics,
        xShmMap = &ShmMap, xShmLock = &ShmLock,
        xShmBarrier = &ShmBarrier, xShmUnmap = &ShmUnmap
    };
    private static readonly sqlite3_vfs VfsTable = new()
    {
        iVersion = 2, szOsFile = sizeof(FileRecord), mxPathname = PathBytes - 1,
        xOpen = &Open, xDelete = &Delete, xAccess = &Access,
        xFullPathname = &FullPath, xRandomness = &Randomness, xSleep = &Sleep,
        xCurrentTime = &CurrentTime, xCurrentTimeInt64 = &CurrentTimeInt64,
        xGetLastError = &GetLastError
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct FileRecord
    {
        public sqlite3_file Base;
        public nint Context;
    }

    private sealed class OpenFile
    {
        public required HostPlatform.FileHandle File;
        public required string Path;
        public bool IsReadOnly;
        public bool NeedsDirectorySync;
        public HostSharedMemory.Connection? SharedMemory;
    }

    public static int OpenHandleCount => Volatile.Read(ref openHandles);

    // Called explicitly by the translated sqlite3_os_init/sqlite3_os_end hooks.
    public static int dotcc_host_vfs_init()
    {
        try
        {
            lock (RegistrationGate)
            {
                if (registeredVfs == null)
                {
                    sqlite3_io_methods* methods = null;
                    sqlite3_vfs* vfs = null;
                    byte* name = null;
                    try
                    {
                        methods = (sqlite3_io_methods*)NativeMemory.Alloc((nuint)sizeof(sqlite3_io_methods));
                        vfs = (sqlite3_vfs*)NativeMemory.Alloc((nuint)sizeof(sqlite3_vfs));
                        name = (byte*)NativeMemory.Alloc(11);
                        if (methods == null || vfs == null || name == null) return NoMemory;
                        *methods = MethodTable;
                        *vfs = VfsTable;
                        "dotcc-host\0"u8.CopyTo(new Span<byte>(name, 11));
                        vfs->zName = name;
                        registeredMethods = methods;
                        registeredVfs = vfs;
                    }
                    finally
                    {
                        if (registeredVfs == null)
                        {
                            NativeMemory.Free(methods); NativeMemory.Free(vfs); NativeMemory.Free(name);
                        }
                    }
                }
                return sqlite3_vfs_register(registeredVfs, 1);
            }
        }
        catch (Exception exception) { return Failure(exception, Error); }
    }

    public static int dotcc_host_vfs_end()
    {
        lock (RegistrationGate)
        {
            if (OpenHandleCount != 0) return Busy;
            // Keep the small canonical tables for initialization after shutdown.
            // No live SQLite registration may retain freed callback table memory.
            return registeredVfs == null ? Ok : sqlite3_vfs_unregister(registeredVfs);
        }
    }

    private static OpenFile State(sqlite3_file* file)
        => (OpenFile)GCHandle.FromIntPtr(((FileRecord*)file)->Context).Target!;

    private static int Failure(Exception exception, int result)
    {
        lastError = exception.Message;
        lastErrorNumber = exception.HResult;
        if (exception is OutOfMemoryException) return NoMemory;
        // HRESULT_FROM_WIN32 on Windows and the native errno preserved by Unix IO.
        var code = exception.HResult & 0xffff;
        if (exception is IOException && (OperatingSystem.IsWindows() ? code is 112 or 39 : code == 28)) return Full;
        return result;
    }

    private static string Decode(byte* text)
        => Marshal.PtrToStringUTF8((nint)text) ?? throw new ArgumentNullException(nameof(text));

    private static int Open(sqlite3_vfs* vfs, byte* name, sqlite3_file* file, int flags, int* outputFlags)
    {
        file->pMethods = null;
        ((FileRecord*)file)->Context = 0;
        HostPlatform.FileHandle? handle = null;
        try
        {
            var temporary = name == null;
            var path = temporary ? System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dotcc-sqlite-" + Guid.NewGuid().ToString("N"))
                : System.IO.Path.GetFullPath(Decode(name));
            if (Directory.Exists(path)) return CannotOpen;
            var readOnly = (flags & OpenReadOnly) != 0;
            if (readOnly == ((flags & OpenReadWrite) != 0)) return CannotOpen;
            var delete = temporary || (flags & DeleteOnClose) != 0;
            var mode = temporary || (flags & (OpenCreate | Exclusive)) == (OpenCreate | Exclusive)
                ? FileMode.CreateNew : (flags & OpenCreate) != 0 ? FileMode.OpenOrCreate : FileMode.Open;
            if (readOnly && mode != FileMode.Open) return CannotOpen;
            var options = delete ? FileOptions.DeleteOnClose : FileOptions.None;
            try
            {
                handle = HostPlatform.Open(path, mode, readOnly ? FileAccess.Read : FileAccess.ReadWrite,
                    options, (flags & NoFollow) != 0);
            }
            catch (Exception exception) when (!readOnly && !temporary && (flags & MainDb) != 0 && (flags & Exclusive) == 0
                && exception is IOException or UnauthorizedAccessException)
            {
                // Existing readable databases may be opened without write access.
                handle = HostPlatform.Open(path, FileMode.Open, FileAccess.Read, options, (flags & NoFollow) != 0);
                readOnly = true;
            }
            var state = new OpenFile { File = handle, Path = path, IsReadOnly = readOnly,
                NeedsDirectorySync = !delete && !readOnly && (flags & OpenCreate) != 0 };
            var context = GCHandle.Alloc(state);
            ((FileRecord*)file)->Context = GCHandle.ToIntPtr(context);
            file->pMethods = registeredMethods;
            Interlocked.Increment(ref openHandles);
            if (outputFlags != null) *outputFlags = (flags & ~(OpenReadOnly | OpenReadWrite)) | (readOnly ? OpenReadOnly : OpenReadWrite);
            return Ok;
        }
        catch (Exception exception)
        {
            try { handle?.Dispose(); } catch { /* Preserve the original open failure. */ }
            return Failure(exception, CannotOpen);
        }
    }

    private static int Close(sqlite3_file* file)
    {
        try
        {
            var state = State(file);
            try { return ShmUnmap(file, 0); }
            finally { state.File.Dispose(); }
        }
        catch (Exception exception) { return Failure(exception, IoError | (16 << 8)); }
        finally
        {
            GCHandle.FromIntPtr(((FileRecord*)file)->Context).Free();
            ((FileRecord*)file)->Context = 0;
            file->pMethods = null;
            Interlocked.Decrement(ref openHandles);
        }
    }

    private static bool ValidRange(int count, long offset) => count >= 0 && offset >= 0 && offset <= long.MaxValue - count;

    private static int Read(sqlite3_file* file, void* buffer, int count, long offset)
    {
        try
        {
            if (!ValidRange(count, offset)) return IoError | (1 << 8);
            var destination = new Span<byte>(buffer, count);
            var total = 0;
            while (total < count)
            {
                var read = RandomAccess.Read(State(file).File.Handle, destination[total..], offset + total);
                if (read == 0)
                {
                    destination[total..].Clear();
                    return IoError | (2 << 8);
                }
                total += read;
            }
            return Ok;
        }
        catch (Exception exception) { return Failure(exception, IoError | (1 << 8)); }
    }

    private static int Write(sqlite3_file* file, void* buffer, int count, long offset)
    {
        try
        {
            if (State(file).IsReadOnly) return ReadOnly;
            if (!ValidRange(count, offset)) return IoError | (3 << 8);
            RandomAccess.Write(State(file).File.Handle, new ReadOnlySpan<byte>(buffer, count), offset);
            return Ok;
        }
        catch (Exception exception) { return Failure(exception, IoError | (3 << 8)); }
    }

    private static int Truncate(sqlite3_file* file, long size)
    {
        try
        {
            if (State(file).IsReadOnly) return ReadOnly;
            RandomAccess.SetLength(State(file).File.Handle, size);
            return Ok;
        }
        catch (Exception exception) { return Failure(exception, IoError | (6 << 8)); }
    }

    private static int Sync(sqlite3_file* file, int flags)
    {
        try
        {
            var state = State(file);
            HostPlatform.Sync(state.File.Handle, (flags & 0xf) == 3);
            if (state.NeedsDirectorySync)
            {
                try { HostPlatform.SyncDirectory(System.IO.Path.GetDirectoryName(state.Path)!); }
                catch (Exception exception) { return Failure(exception, IoError | (5 << 8)); }
                state.NeedsDirectorySync = false;
            }
            return Ok;
        }
        catch (Exception exception) { return Failure(exception, IoError | (4 << 8)); }
    }

    private static int FileSize(sqlite3_file* file, long* size)
    {
        try { *size = RandomAccess.GetLength(State(file).File.Handle); return Ok; }
        catch (Exception exception) { return Failure(exception, IoError | (7 << 8)); }
    }

    private static int Lock(sqlite3_file* file, int level)
    {
        try { return State(file).File.Lock(level); }
        catch (Exception exception) { return Failure(exception, IoError | (15 << 8)); }
    }
    private static int Unlock(sqlite3_file* file, int level)
    {
        try { return State(file).File.Unlock(level); }
        catch (Exception exception) { return Failure(exception, IoError | (8 << 8)); }
    }
    private static int CheckReserved(sqlite3_file* file, int* result)
    {
        *result = 0;
        try
        {
            var rc = State(file).File.CheckReserved(out var reserved);
            *result = reserved ? 1 : 0;
            return rc;
        }
        catch (Exception exception) { return Failure(exception, IoError | (14 << 8)); }
    }

    private static int ShmMap(sqlite3_file* file, int region, int size, int extend, void** output)
    {
        *output = null;
        try
        {
            var state = State(file);
            if (state.SharedMemory == null)
            {
                var opened = HostSharedMemory.Open(state.File, state.Path, out state.SharedMemory);
                if (opened != Ok) return opened;
            }
            var result = state.SharedMemory!.Map(region, size, extend != 0, out var address);
            *output = (void*)address;
            return result;
        }
        catch (Exception exception) { return Failure(exception, IoError | (21 << 8)); }
    }

    private static int ShmLock(sqlite3_file* file, int offset, int count, int flags)
    {
        try { return State(file).SharedMemory?.Lock(offset, count, flags) ?? (IoError | (20 << 8)); }
        catch (Exception exception) { return Failure(exception, IoError | (20 << 8)); }
    }

    private static void ShmBarrier(sqlite3_file* file) => HostSharedMemory.Barrier();

    private static int ShmUnmap(sqlite3_file* file, int delete)
    {
        var state = State(file);
        try { return state.SharedMemory?.Unmap(delete != 0) ?? Ok; }
        catch (Exception exception) { return Failure(exception, IoError | (22 << 8)); }
        finally { state.SharedMemory = null; }
    }

    private static int FileControl(sqlite3_file* file, int operation, void* argument)
    {
        try
        {
            switch (operation)
            {
                case 1: *(int*)argument = State(file).File.LockLevel; return Ok; // LOCKSTATE
                case 4: *(int*)argument = lastErrorNumber; return Ok; // LAST_ERRNO
                case 5: return Ok; // SIZE_HINT is an optional allocation hint.
                case 18: *(long*)argument = 0; return Ok; // MMAP_SIZE
                default: return NotFound;
            }
        }
        catch (Exception exception) { return Failure(exception, IoError); }
    }
    private static int SectorSize(sqlite3_file* file) => 4096;
    private static int DeviceCharacteristics(sqlite3_file* file) => 0;

    private static int Delete(sqlite3_vfs* vfs, byte* name, int syncDirectory)
    {
        try
        {
            var path = System.IO.Path.GetFullPath(Decode(name));
            File.Delete(path);
            if (syncDirectory != 0)
            {
                try { HostPlatform.SyncDirectory(System.IO.Path.GetDirectoryName(path)!); }
                catch (Exception exception) { return Failure(exception, IoError | (5 << 8)); }
            }
            return Ok;
        }
        catch (Exception exception) { return Failure(exception, IoError | (10 << 8)); }
    }
    private static int Access(sqlite3_vfs* vfs, byte* name, int flags, int* result)
    {
        *result = 0;
        try { *result = HostPlatform.Access(Decode(name), flags) ? 1 : 0; return Ok; }
        catch (Exception exception) { return Failure(exception, IoError | (13 << 8)); }
    }
    private static int FullPath(sqlite3_vfs* vfs, byte* name, int count, byte* output)
    {
        try
        {
            var path = CanonicalPath(Decode(name), out var followedLink);
            var size = Encoding.UTF8.GetByteCount(path);
            if (count <= size) return CannotOpen;
            var span = new Span<byte>(output, count);
            Encoding.UTF8.GetBytes(path, span);
            span[size] = 0;
            // The pager turns this into CANTOPEN_SYMLINK for OPEN_NOFOLLOW,
            // otherwise accepts the canonical path for the database and journals.
            return followedLink ? 512 : Ok; // SQLITE_OK_SYMLINK
        }
        catch (Exception exception) { return Failure(exception, CannotOpen); }
    }

    private static string CanonicalPath(string path, out bool followedLink)
    {
        // Resolve each component before '..': collapsing it before following a
        // link can select a different database and therefore a different journal.
        var absolute = System.IO.Path.IsPathFullyQualified(path) ? path
            : OperatingSystem.IsWindows() ? System.IO.Path.GetFullPath(path)
            : System.IO.Path.Combine(Environment.CurrentDirectory, path);
        var root = System.IO.Path.GetPathRoot(absolute)!;
        var current = root;
        var separators = new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };
        var components = new Queue<string>(absolute[root.Length..].Split(separators, StringSplitOptions.RemoveEmptyEntries));
        var links = 0;
        followedLink = false;
        while (components.TryDequeue(out var component))
        {
            if (component == ".") continue;
            if (component == "..")
            {
                if (current != root) current = System.IO.Path.GetDirectoryName(current.TrimEnd(separators)) ?? root;
                continue;
            }
            var candidate = System.IO.Path.Combine(current, component);
            string? target;
            try { target = new FileInfo(candidate).LinkTarget; }
            catch (FileNotFoundException) { target = null; }
            catch (DirectoryNotFoundException) { target = null; }
            if (target == null) { current = candidate; continue; }
            if (++links > 40) throw new IOException("Too many symbolic links in SQLite database path.");
            followedLink = true;
            var replacement = System.IO.Path.IsPathFullyQualified(target) ? target : System.IO.Path.Combine(current, target);
            root = System.IO.Path.GetPathRoot(replacement)!;
            current = root;
            components = new Queue<string>(replacement[root.Length..].Split(separators, StringSplitOptions.RemoveEmptyEntries).Concat(components));
        }
        return current;
    }
    private static int Randomness(sqlite3_vfs* vfs, int count, byte* output)
    {
        try { RandomNumberGenerator.Fill(new Span<byte>(output, count)); return count; }
        catch (Exception exception) { Failure(exception, Error); return 0; }
    }
    private static int Sleep(sqlite3_vfs* vfs, int microseconds)
    {
        try
        {
            if (microseconds <= 0) return 0;
            var milliseconds = (int)(((long)microseconds + 999) / 1000);
            Thread.Sleep(milliseconds);
            return (int)Math.Min(int.MaxValue, (long)milliseconds * 1000);
        }
        catch (Exception exception) { Failure(exception, Error); return 0; }
    }
    private static int CurrentTimeInt64(sqlite3_vfs* vfs, long* result)
    {
        try { *result = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 210866760000000L; return Ok; }
        catch (Exception exception) { return Failure(exception, Error); }
    }
    private static int CurrentTime(sqlite3_vfs* vfs, double* result)
    {
        long milliseconds = 0;
        var rc = CurrentTimeInt64(vfs, &milliseconds);
        *result = milliseconds / 86400000.0;
        return rc;
    }
    private static int GetLastError(sqlite3_vfs* vfs, int count, byte* output)
    {
        try
        {
            if (count > 0)
            {
                var bytes = Encoding.UTF8.GetBytes(lastError ?? "");
                var copied = Math.Min(bytes.Length, count - 1);
                bytes.AsSpan(0, copied).CopyTo(new Span<byte>(output, copied));
                output[copied] = 0;
            }
            return lastErrorNumber;
        }
        catch { if (count > 0) output[0] = 0; return Error; }
    }
}
