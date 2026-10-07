#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Edg.Runtime;

public sealed unsafe partial class PosixContext
{
    public sbyte* Realpath(sbyte* path, sbyte* destination, Func<ulong, nint> allocate)
    {
        if (path == null) { SetError(22); return null; }
        try
        {
            string resolved = PortablePath(PosixPlatform.CanonicalPath(Resolve(path)));
            int length = Utf8.GetByteCount(resolved);
            // The translated ABI uses the same PATH_MAX on every host.
            if (length >= 4096) { SetError(36); return null; }
            if (destination == null)
            {
                destination = (sbyte*)allocate((ulong)length + 1);
                if (destination == null) { SetError(12); return null; }
            }
            Utf8.GetBytes(resolved, new Span<byte>(destination, length));
            destination[length] = 0;
            return destination;
        }
        catch (Exception error) when (PlatformFailure(error)) { Fail(error); return null; }
    }

    private readonly object directoriesGate = new();
    private readonly Dictionary<nint, DirectoryOwner> directories = new();
    private sealed class DirectoryOwner
    {
        internal readonly object Gate = new();
        internal IEnumerator<string>? Entries;
        internal bool Pending;
        internal int Dots;
        internal byte* Entry;
    }

    public int Chdir(sbyte* path)
    {
        try
        {
            string next = Resolve(path);
            if ((File.GetAttributes(next) & FileAttributes.Directory) == 0) { SetError(20); return -1; }
            // Resolve '.' and '..' through the filesystem, retaining symlink
            // traversal semantics before saving a canonical owner cwd.
            if (!OperatingSystem.IsWindows())
            {
                nint resolved = Realpath(next, 0);
                if (resolved == 0) { SetError(PosixPlatform.Error(new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError()))); return -1; }
                try { next = Marshal.PtrToStringUTF8(resolved)!; } finally { FreePath(resolved); }
            }
            else next = Path.GetFullPath(next);
            workingDirectory = next; return 0;
        }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }

    public int Mkstemp(sbyte* template)
    {
        if (template == null) { SetError(14); return -1; }
        int size = 0; while (template[size] != 0) size++;
        if (size < 6) { SetError(22); return -1; }
        for (int i = size - 6; i < size; ++i) if (template[i] != 'X') { SetError(22); return -1; }
        ReadOnlySpan<byte> alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"u8;
        Span<byte> random = stackalloc byte[6];
        for (int attempt = 0; attempt < 256; ++attempt)
        {
            RandomNumberGenerator.Fill(random);
            for (int i = 0; i < 6; ++i) template[size - 6 + i] = (sbyte)alphabet[random[i] % alphabet.Length];
            int fd = Open(template, 2 | 0x40 | 0x80, 384);
            if (fd >= 0) return fd;
            if (lastError != 17) return -1;
        }
        SetError(17); return -1;
    }

    public int Isatty(int fd)
    {
        var descriptor = Find(fd); if (descriptor is null) return 0;
        lock (descriptor.Gate)
        {
            if (descriptor.Closed) { SetError(9); return 0; }
            if (descriptor.Terminal) return 1;
            if (descriptor.Device == 0 && !OperatingSystem.IsWindows())
            {
                if (IsattyUnix((int)descriptor.Handle.DangerousGetHandle()) != 0) return 1;
            }
            SetError(25); return 0;
        }
    }

    public void* Opendir(sbyte* path)
    {
        IEnumerator<string>? enumerator = null;
        byte* entry = null; void* token = null;
        try
        {
            string name = Resolve(path);
            if ((File.GetAttributes(name) & FileAttributes.Directory) == 0) { SetError(20); return null; }
            enumerator = Directory.EnumerateFileSystemEntries(name).GetEnumerator();
            bool pending = enumerator.MoveNext();
            entry = (byte*)NativeMemory.Alloc(4096); token = NativeMemory.Alloc(1);
            if (entry == null || token == null) throw new OutOfMemoryException();
            lock (directoriesGate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                directories.Add((nint)token, new DirectoryOwner { Entries = enumerator, Pending = pending, Entry = entry });
            }
            void* result = token; token = null; entry = null; enumerator = null; return result;
        }
        catch (Exception error) when (PlatformFailure(error)) { Fail(error); return null; }
        finally { enumerator?.Dispose(); NativeMemory.Free(entry); NativeMemory.Free(token); }
    }
    public void* Readdir(void* token)
    {
        DirectoryOwner? directory;
        lock (directoriesGate) { if (!directories.TryGetValue((nint)token, out directory)) { SetError(9); return null; } }
        lock (directory.Gate)
        {
            if (directory.Entries is null) { SetError(9); return null; }
            try
            {
                string name;
                if (directory.Dots < 2) name = directory.Dots++ == 0 ? "." : "..";
                else
                {
                    if (!directory.Pending && !directory.Entries.MoveNext()) return null;
                    directory.Pending = false; name = Path.GetFileName(directory.Entries.Current);
                }
                int size = Utf8.GetByteCount(name);
                if (size >= 4096) { SetError(36); return null; }
                Utf8.GetBytes(name, new Span<byte>(directory.Entry, size)); directory.Entry[size] = 0;
                return directory.Entry;
            }
            catch (Exception error) when (PlatformFailure(error)) { Fail(error); return null; }
        }
    }
    public int Closedir(void* token)
    {
        DirectoryOwner? directory;
        lock (directoriesGate) { if (!directories.TryGetValue((nint)token, out directory)) { SetError(9); return -1; } }
        lock (directory.Gate)
        {
            if (directory.Entries is null) { SetError(9); return -1; }
            directory.Entries.Dispose(); directory.Entries = null;
            NativeMemory.Free(directory.Entry); directory.Entry = null; return 0;
        }
    }
    private void CloseDirectories()
    {
        lock (directoriesGate)
        {
            foreach (var item in directories)
            {
                lock (item.Value.Gate)
                { item.Value.Entries?.Dispose(); NativeMemory.Free(item.Value.Entry); NativeMemory.Free((void*)item.Key); }
            }
            directories.Clear();
        }
    }
    [LibraryImport("libc", EntryPoint = "isatty", SetLastError = true)] private static partial int IsattyUnix(int fd);
    [LibraryImport("libc", EntryPoint = "realpath", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)] private static partial nint Realpath(string path, nint result);
    [LibraryImport("libc", EntryPoint = "free")] private static partial void FreePath(nint path);
}
