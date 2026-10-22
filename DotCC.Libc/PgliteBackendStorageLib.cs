#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // Try exclusive creation first, so fchmod can never alter an existing
    // object's permissions. The existing-object open has O_CREAT removed.
    // Resolve a dangling final symlink only after that open reports ENOENT;
    // ordinary symlinks, O_NOFOLLOW and O_EXCL retain kernel semantics.
    private static int OpenLinuxCreatedDescriptor(string path, int flags, uint mode)
    {
        const int create = 0x40, exclusive = 0x80, nofollow = 0x20000;
        if ((flags & create) == 0) return BackendOpenPath(path, flags, mode);
        uint desired;
        try { desired = ApplyCreationMode(mode) & 0xfff; }
        catch (PlatformNotSupportedException) { errno = ENOTSUP; return -1; }
        catch (UnauthorizedAccessException) { errno = EACCES; return -1; }
        catch (IOException) { errno = EIO; return -1; }
        string current = path;
        int links = 0;
        for (int attempt = 0; attempt < 64; attempt++)
        {
            int descriptor = BackendOpenPath(current, flags | exclusive, desired);
            if (descriptor >= 0)
            {
                if (BackendNativeFchmod(descriptor, desired) == 0) return descriptor;
                int error = Marshal.GetLastPInvokeError();
                BackendNativeClose(descriptor);
                errno = error;
                return -1;
            }
            if (errno != EEXIST || (flags & exclusive) != 0) return -1;
            descriptor = BackendOpenPath(current, flags & ~create, 0);
            if (descriptor >= 0 || errno != ENOENT || (flags & nofollow) != 0) return descriptor;
            // A final symlink may exist even though its target does not.
            // FileInfo.LinkTarget reads the link rather than its target. Keep
            // '..' components for the kernel to resolve through directory links.
            try
            {
                string? target = new FileInfo(current).LinkTarget;
                if (target is null) continue; // concurrent unlink; bounded retry
                if (++links > 40) { errno = ELOOP; return -1; }
                current = global::System.IO.Path.IsPathRooted(target) ? target :
                    global::System.IO.Path.Combine(global::System.IO.Path.GetDirectoryName(current) ?? ".", target);
            }
            catch (UnauthorizedAccessException) { errno = EACCES; return -1; }
            catch (IOException) { errno = EIO; return -1; }
        }
        errno = EAGAIN;
        return -1;
    }

    private static int BackendOpenPath(string path, int flags, uint mode)
    {
        int descriptor;
        fixed (byte* name = Encoding.UTF8.GetBytes(path + "\0")) descriptor = PosixOpenFile(name, flags, (int)mode);
        if (descriptor < 0) errno = Marshal.GetLastPInvokeError();
        return descriptor;
    }

    private static int PgliteBackendMkdir(byte* path, uint mode)
    {
        if (path == null) { errno = EFAULT; return -1; }
        try
        {
            string resolved = ResolvePath(path);
            uint desired = ApplyCreationMode(mode) & 0xfff;
            fixed (byte* name = Encoding.UTF8.GetBytes(resolved + "\0"))
                if (BackendNativeMkdir(name, desired) != 0)
                { errno = Marshal.GetLastPInvokeError(); return -1; }
            // O_PATH|O_DIRECTORY|O_NOFOLLOW pins the directory even for mode
            // 0000. chmod through procfs follows this owned descriptor, never
            // a later path replacement. A failed mode application stays an error.
            int descriptor = BackendOpenPath(resolved, 0x200000 | 0x10000 | 0x20000 | 0x80000, 0);
            if (descriptor < 0) return -1;
            try
            {
                string descriptorPath = "/proc/self/fd/" + descriptor.ToString(global::System.Globalization.CultureInfo.InvariantCulture);
                fixed (byte* name = Encoding.UTF8.GetBytes(descriptorPath + "\0"))
                    if (BackendNativeChmod(name, desired) != 0)
                    { errno = Marshal.GetLastPInvokeError(); return -1; }
                return 0;
            }
            finally { BackendNativeClose(descriptor); }
        }
        catch (PlatformNotSupportedException) { errno = ENOTSUP; return -1; }
        catch (UnauthorizedAccessException) { errno = EACCES; return -1; }
        catch (IOException) { errno = EIO; return -1; }
        catch (ArgumentException) { errno = EINVAL; return -1; }
    }

    public static int fdatasync(int descriptor)
    {
        var slot = SlotByFd(descriptor);
        if (slot is null) { errno = EBADF; return -1; }
        if (slot.Stream is not FileStream file) { errno = EINVAL; return -1; }
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
        { errno = ENOTSUP; return -1; }
        try
        {
            if (!FlushStdioBuffer(slot)) return -1;
            slot.Writer?.Flush();
            file.Flush();
            if (BackendNativeFdatasync(file.SafeFileHandle) == 0) return 0;
            errno = Marshal.GetLastPInvokeError();
            return -1;
        }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
        catch (IOException) { errno = EIO; return -1; }
        catch (EntryPointNotFoundException) { errno = ENOSYS; return -1; }
        catch (DllNotFoundException) { errno = ENOSYS; return -1; }
    }

    // LP64 off_t and long have the same width. Validate whence before the
    // existing stdio seek, which already flushes and resets buffered input.
    public static int fseeko(FILE* stream, long offset, int whence)
    {
        if (Slot(stream) is null) { errno = EBADF; return -1; }
        if (whence is < 0 or > 2) { errno = EINVAL; return -1; }
        try { return fseek(stream, offset, whence); }
        catch (ArgumentException) { errno = EINVAL; return -1; }
        catch (OverflowException) { errno = EOVERFLOW; return -1; }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
    }

    public static long copy_file_range(int input, long* inputOffset, int output, long* outputOffset, ulong count, uint flags)
    {
        var source = SlotByFd(input);
        var destination = SlotByFd(output);
        if (source is null || destination is null) { errno = EBADF; return -1; }
        if (source.Stream is not FileStream reader || destination.Stream is not FileStream writer)
        { errno = EINVAL; return -1; }
        if (!reader.CanRead || !writer.CanWrite) { errno = EBADF; return -1; }
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
        { errno = ENOTSUP; return -1; }
        try
        {
            // FileStream tracks its logical cursor independently of the OS fd
            // cursor. Use explicit native offsets, then update only the cursors
            // that the C call requested. Supplied offset pointers do not seek.
            long from = inputOffset == null ? reader.Position : *inputOffset;
            long to = outputOffset == null ? writer.Position : *outputOffset;
            long result = BackendNativeCopyFileRange(reader.SafeFileHandle, &from, writer.SafeFileHandle, &to, count, flags);
            if (result < 0) errno = Marshal.GetLastPInvokeError();
            else
            {
                if (inputOffset == null) reader.Position = from; else *inputOffset = from;
                if (outputOffset == null) writer.Position = to; else *outputOffset = to;
            }
            return result;
        }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
        catch (IOException) { errno = EIO; return -1; }
        catch (ArgumentException) { errno = EINVAL; return -1; }
        catch (EntryPointNotFoundException) { errno = ENOSYS; return -1; }
        catch (DllNotFoundException) { errno = ENOSYS; return -1; }
    }

    /// <summary>Reserve real filesystem space on the owner's existing open
    /// file. POSIX returns the error number directly and preserves errno.</summary>
    public static int posix_fallocate(int descriptor, long offset, long length)
    {
        int savedError = errno;
        try
        {
            if (offset < 0 || length <= 0) return EINVAL;
            if (offset > long.MaxValue - length) return EFBIG;
            var slot = SlotByFd(descriptor);
            if (slot is null) return EBADF;
            if (slot.Stream is not FileStream file) return ESPIPE;
            if (!file.CanWrite) return EBADF;
            if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
                return ENOTSUP;
            // SafeHandle marshalling retains the kernel descriptor throughout
            // libc's real allocation/fallback algorithm. No seeking or zero-
            // length truncation is used as a substitute for reserving space.
            return BackendNativeFallocate(file.SafeFileHandle, offset, length);
        }
        catch (ObjectDisposedException) { return EBADF; }
        catch (EntryPointNotFoundException) { return ENOSYS; }
        catch (DllNotFoundException) { return ENOSYS; }
        catch (IOException) { return EIO; }
        finally { errno = savedError; }
    }

    [DllImport("libc", EntryPoint = "posix_fallocate")]
    private static extern int BackendNativeFallocate(SafeFileHandle descriptor, long offset, long length);

    [DllImport("libc", EntryPoint = "fchmod", SetLastError = true)]
    private static extern int BackendNativeFchmod(int descriptor, uint mode);
    [DllImport("libc", EntryPoint = "close", SetLastError = true)]
    private static extern int BackendNativeClose(int descriptor);
    [DllImport("libc", EntryPoint = "mkdir", SetLastError = true)]
    private static extern int BackendNativeMkdir(byte* path, uint mode);
    [DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
    private static extern int BackendNativeChmod(byte* path, uint mode);
    [DllImport("libc", EntryPoint = "fdatasync", SetLastError = true)]
    private static extern int BackendNativeFdatasync(SafeFileHandle descriptor);
    [DllImport("libc", EntryPoint = "copy_file_range", SetLastError = true)]
    private static extern long BackendNativeCopyFileRange(SafeFileHandle input, long* inputOffset, SafeFileHandle output, long* outputOffset, ulong count, uint flags);
}
