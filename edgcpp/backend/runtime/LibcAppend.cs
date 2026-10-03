#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Edg.Runtime;

// .NET 10 SafeFileHandle.Unix.cs maps FileMode.Append to O_CREAT without
// O_APPEND; OSFileStreamStrategy keeps a managed offset. SeekEnd followed by
// Write is not an atomic append. Keep ordinary I/O in FileStream, but use the
// operating system's append operation for each actual append write.
internal static unsafe partial class LibcAppend
{
    internal static FileStream Open(string path, bool readable)
    {
        if (OperatingSystem.IsWindows())
        {
            // Keep normal GENERIC_WRITE rights so fseek/ftell work. WriteFile's
            // all-ones offset requests the same end-of-file operation as an
            // append-only handle without dropping its seeking access rights.
            return new FileStream(path, FileMode.OpenOrCreate, readable ? FileAccess.ReadWrite : FileAccess.Write,
                FileShare.ReadWrite | FileShare.Delete, 1);
        }
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("Atomic append is implemented for Windows, Linux and macOS.");
        var stream = new FileStream(path, FileMode.OpenOrCreate, readable ? FileAccess.ReadWrite : FileAccess.Write,
            FileShare.ReadWrite | FileShare.Delete, 1);
        try
        {
            int fd = stream.SafeFileHandle.DangerousGetHandle().ToInt32();
            int flags = FcntlCommand(fd, 3, 0); // F_GETFL
            if (flags < 0 || FcntlCommand(fd, 4, flags | (OperatingSystem.IsMacOS() ? 8 : 1024)) < 0) throw NativeError();
            return stream;
        }
        catch { stream.Dispose(); throw; }
    }

    internal static void SetAppend(FileStream stream, bool append)
    {
        if (OperatingSystem.IsWindows()) return; // Write chooses its explicit EOF offset.
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("Atomic append is implemented for Windows, Linux and macOS.");
        int fd = stream.SafeFileHandle.DangerousGetHandle().ToInt32();
        int flags = FcntlCommand(fd, 3, 0), flag = OperatingSystem.IsMacOS() ? 8 : 1024;
        if (flags < 0 || FcntlCommand(fd, 4, append ? flags | flag : flags & ~flag) < 0)
            throw NativeError();
        GC.KeepAlive(stream);
    }

    internal static void Write(FileStream stream, ReadOnlySpan<byte> bytes)
    {
        // Accessing SafeFileHandle synchronizes FileStream's cached offset with
        // the OS descriptor. Native append ignores that offset for the write.
        var handle = stream.SafeFileHandle;
        fixed (byte* start = bytes)
        {
            int total = 0;
            try
            {
                while (total < bytes.Length)
                {
                    long written;
                    if (OperatingSystem.IsWindows())
                    {
                        WindowsOverlapped append = default;
                        append.Offset = append.OffsetHigh = uint.MaxValue;
                        if (WriteFile(handle, start + total, (uint)(bytes.Length - total), out uint count, &append) == 0)
                            throw NativeError();
                        written = count;
                    }
                    else
                    {
                        nint count = WriteUnix(handle.DangerousGetHandle().ToInt32(), start + total, (nuint)(bytes.Length - total));
                        if (count < 0 && Marshal.GetLastPInvokeError() == 4) continue; // EINTR before any bytes
                        if (count < 0) throw NativeError();
                        written = count;
                    }
                    if (written == 0) throw new IOException("Append made no progress.");
                    total = checked(total + (int)written);
                }
                // Restore the managed cached position to this descriptor's
                // actual post-write offset, not another writer's new EOF.
                long position;
                if (OperatingSystem.IsWindows())
                {
                    if (SetFilePointer(handle, 0, out position, 1) == 0) throw NativeError();
                }
                else
                {
                    position = Lseek(handle.DangerousGetHandle().ToInt32(), 0, 1);
                    if (position < 0) throw NativeError();
                }
                stream.Position = position;
            }
            finally { GC.KeepAlive(stream); }
        }
    }

    private static IOException NativeError()
    {
        int error = Marshal.GetLastPInvokeError();
        return new IOException("Native append failed (platform error " + error + ").", error);
    }

    private static int FcntlCommand(int fd, int command, int argument)
    {
        // Apple arm64 passes the variadic portion on the stack. Reserve the
        // remaining integer argument registers so the final fixed-signature
        // P/Invoke argument occupies that first stack slot. Native fcntl ignores
        // those register values. Linux arm64 and x64 use the ordinary entry.
        if (OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            return FcntlAppleArm64(fd, command, 0, 0, 0, 0, 0, 0, (nint)(uint)argument);
        return Fcntl(fd, command, argument);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsOverlapped
    {
        internal nuint Internal, InternalHigh;
        internal uint Offset, OffsetHigh;
        internal nint Event;
    }

    [LibraryImport("libc", EntryPoint = "fcntl", SetLastError = true)]
    private static partial int Fcntl(int fd, int command, int argument);
    [LibraryImport("libc", EntryPoint = "fcntl", SetLastError = true)]
    private static partial int FcntlAppleArm64(int fd, int command, nint x2, nint x3, nint x4, nint x5, nint x6, nint x7, nint argument);
    [LibraryImport("libc", EntryPoint = "write", SetLastError = true)]
    private static partial nint WriteUnix(int fd, void* buffer, nuint count);
    [LibraryImport("libc", EntryPoint = "lseek", SetLastError = true)]
    private static partial long Lseek(int fd, long offset, int origin);
    [LibraryImport("kernel32.dll", EntryPoint = "WriteFile", SetLastError = true)]
    private static partial int WriteFile(SafeFileHandle file, void* buffer, uint count, out uint written, void* overlapped);
    [LibraryImport("kernel32.dll", EntryPoint = "SetFilePointerEx", SetLastError = true)]
    private static partial int SetFilePointer(SafeFileHandle file, long distance, out long position, uint origin);
}
