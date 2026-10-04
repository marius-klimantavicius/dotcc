#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Edg.Runtime;

/// <summary>Descriptor and working-directory state for one translated Unix process.
/// The portable ABI is independent of the host's native POSIX structure layouts.</summary>
public sealed unsafe partial class PosixContext : IDisposable, ILibcFileDescriptors
{
    internal readonly Action<int> SetError;
    internal readonly object LockOwner = new();
    private readonly object descriptorsGate = new();
    private readonly Dictionary<int, Descriptor> descriptors = new();
    private string workingDirectory;
    [ThreadStatic] private static int lastError;
    private bool disposed;
    internal static readonly UTF8Encoding Utf8 = new(false, true);

    internal sealed class Descriptor
    {
        internal readonly SafeFileHandle Handle;
        internal readonly string Path;
        internal readonly object Gate = new();
        internal long Position;
        internal int Flags;
        internal int DescriptorFlags;
        internal bool Closed;
        // Devices use no host file descriptor. This also makes /dev/urandom
        // usable on Windows without exposing a platform path to translated code.
        internal readonly int Device;
        internal Stream? StandardStream;
        internal bool Terminal, NativeStandard;
        internal Descriptor(SafeFileHandle handle, string path, int flags, int device = 0)
        { Handle = handle; Path = path; Flags = flags; Device = device; DescriptorFlags = (flags & 0x80000) != 0 ? 1 : 0; }
    }

    public PosixContext(Action<int> setErrno, string? workingDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(setErrno);
        SetError = error => { lastError = error; setErrno(error); };
        this.workingDirectory = System.IO.Path.GetFullPath(workingDirectory ?? Environment.CurrentDirectory);
    }

    public string WorkingDirectory => workingDirectory;

    internal Descriptor? Find(int fd)
    {
        lock (descriptorsGate)
        {
            if (!disposed && descriptors.TryGetValue(fd, out Descriptor? descriptor)) return descriptor;
        }
        SetError(9); return null;
    }

    internal string Resolve(sbyte* path)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        int size = 0; while (path[size] != 0) size = checked(size + 1);
        string name = Utf8.GetString(new ReadOnlySpan<byte>(path, size));
        if (name.Length == 0) throw new FileNotFoundException("Empty Unix path.");
        // Do not collapse symlink/.. components: the kernel must resolve them
        // in order, as required by POSIX path traversal.
        if (OperatingSystem.IsWindows()) return ResolveWindowsPath(name, workingDirectory);
        return System.IO.Path.IsPathRooted(name) ? name : System.IO.Path.Combine(workingDirectory, name);
    }

    internal static string ResolveWindowsPath(string name, string directory)
    {
        if (name == "/tmp" || name.StartsWith("/tmp/", StringComparison.Ordinal))
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), name.Length > 5 ? name[5..].Replace('/', '\\') : "");
        if (name.Length >= 3 && name[0] == '/' && char.IsAsciiLetter(name[1]) && name[2] == ':')
            return name.Length == 3 ? name[1..] + "\\" : name[1..].Replace('/', '\\');
        if (name.StartsWith("/unc/", StringComparison.Ordinal)) return "\\\\" + name[5..].Replace('/', '\\');
        if (name.StartsWith('/')) return System.IO.Path.GetPathRoot(directory)! + name[1..].Replace('/', '\\');
        return System.IO.Path.IsPathRooted(name) ? name : System.IO.Path.Combine(directory, name);
    }

    // Unix VFS clients must receive Unix separators even on a Windows host.
    // Drive roots are /C:/ and UNC paths /unc/server/share/. Relative paths
    // remain relative so readlink retains its POSIX interpretation.
    internal static string PortablePath(string path)
    {
        return OperatingSystem.IsWindows() ? PortableWindowsPath(path) : path;
    }
    internal static string PortableWindowsPath(string path)
    {
        string result = path.Replace('\\', '/');
        if (result.StartsWith("//", StringComparison.Ordinal)) return "/unc/" + result[2..];
        return result.Length >= 2 && result[1] == ':' ? "/" + result : result;
    }

    internal int Fail(Exception error) { SetError(PosixPlatform.Error(error)); return -1; }
    internal static bool PlatformFailure(Exception error) => error is IOException or UnauthorizedAccessException
        or ArgumentException or NotSupportedException or OutOfMemoryException or OverflowException or ObjectDisposedException or System.ComponentModel.Win32Exception;

    public int Open(sbyte* path, int flags, int mode = 438)
    {
        SafeFileHandle? handle = null;
        try
        {
            string name = Resolve(path);
            int access = flags & 3;
            if (access == 3) { SetError(22); return -1; }
            int pathLength = 0; while (path[pathLength] != 0) pathLength = checked(pathLength + 1);
            string literal = Utf8.GetString(new ReadOnlySpan<byte>(path, pathLength));
            int device = literal == "/dev/null" ? 1 : literal is "/dev/urandom" or "/dev/random" ? 2 : 0;
            handle = device == 0 ? PosixPlatform.Open(name, flags, mode) : new SafeFileHandle(-1, false);
            var descriptor = new Descriptor(handle, name, flags, device);
            lock (descriptorsGate)
            {
                if (disposed) { handle.Dispose(); SetError(9); return -1; }
                int fd = 3;
                while (descriptors.ContainsKey(fd)) fd = checked(fd + 1);
                descriptors.Add(fd, descriptor);
                return fd;
            }
        }
        catch (Exception error) when (PlatformFailure(error)) { handle?.Dispose(); return Fail(error); }
    }

    public int Close(int fd)
    {
        Descriptor? descriptor;
        lock (descriptorsGate)
        {
            if (!descriptors.Remove(fd, out descriptor)) { SetError(9); return -1; }
        }
        lock (descriptor.Gate)
        {
            descriptor.Closed = true;
            try
            {
                if (descriptor.Device == 0) PosixLocks.Close(LockOwner, descriptor.Handle, descriptor.Path);
                else descriptor.Handle.Dispose();
                return 0;
            }
            catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
        }
    }

    public int FcntlFlags(int fd, int command, int value = 0)
    {
        Descriptor? descriptor = Find(fd); if (descriptor is null) return -1;
        lock (descriptor.Gate)
        {
            if (descriptor.Closed) { SetError(9); return -1; }
            switch (command)
            {
                case 1: return descriptor.DescriptorFlags;
                case 2: descriptor.DescriptorFlags = value & 1; return 0;
                case 3: return descriptor.Flags & ~(0x40 | 0x80 | 0x200 | 0x80000);
                // Regular file I/O is synchronous; O_NONBLOCK is ineffectual.
                // Append is handled by the descriptor's Write operation.
                case 4:
                    int flags = (descriptor.Flags & ~(0x400 | 0x800)) | (value & (0x400 | 0x800));
                    try { if (descriptor.Device == 0) PosixPlatform.SetStatusFlags(descriptor.Handle, flags); descriptor.Flags = flags; return 0; }
                    catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
                default: SetError(22); return -1;
            }
        }
    }

    public void Dispose()
    {
        int[] ids;
        lock (descriptorsGate)
        {
            if (disposed) return;
            disposed = true;
            ids = new int[descriptors.Count]; descriptors.Keys.CopyTo(ids, 0);
        }
        CloseMappings();
        CloseDirectories();
        foreach (int fd in ids) Close(fd);
    }
}
