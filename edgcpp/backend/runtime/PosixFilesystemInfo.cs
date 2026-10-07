#nullable enable
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Edg.Runtime;

public sealed unsafe partial class PosixContext
{
    public int Statvfs(sbyte* path, PosixStatvfs* destination)
    {
        if (path == null || destination == null) { SetError(14); return -1; }
        try { *destination = PosixPlatform.FilesystemInfo(Resolve(path)); return 0; }
        catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
    }

    public int IoctlVariadic(int fd, ulong request, ReadOnlySpan<VaArg> arguments)
    {
        Descriptor? descriptor = Find(fd);
        if (descriptor is null) return -1;
        lock (descriptor.Gate)
        {
            if (descriptor.Closed) { SetError(9); return -1; }
            if (request != 0x5413) { SetError(25); return -1; }
            try
            {
                PosixWinsize* destination = (PosixWinsize*)Variadic<nuint>(arguments);
                if (destination == null) { SetError(14); return -1; }
                if (descriptor.Device != 0 && !descriptor.NativeStandard) { SetError(25); return -1; }
                *destination = PosixPlatform.TerminalSize(descriptor.Handle);
                return 0;
            }
            catch (Exception error) when (PlatformFailure(error)) { return Fail(error); }
        }
    }
}

internal static unsafe partial class PosixPlatform
{
    internal static PosixStatvfs FilesystemInfo(string path)
    {
        EnsureSupported();
        if (!OperatingSystem.IsWindows())
        {
            // Linux LP64 adds a 24-byte tail after the eleven public words;
            // Darwin LP64 has those same public words without the reserved tail.
            byte* record = stackalloc byte[112];
            NativeMemory.Clear(record, 112);
            if (StatvfsNative(path, record) != 0) throw NativeError();
            ulong* values = (ulong*)record;
            return new PosixStatvfs {
                f_bsize = values[0], f_frsize = values[1], f_blocks = values[2],
                f_bfree = values[3], f_bavail = values[4], f_files = values[5],
                f_ffree = values[6], f_favail = values[7], f_fsid = values[8],
                f_flag = values[9] & 3, f_namemax = values[10],
            };
        }
        // Resolve through the filesystem first, including existence and links.
        string canonical = CanonicalPath(path);
        char* volume = stackalloc char[4096];
        fixed (char* name = canonical)
            if (!GetVolumePathName(name, volume, 4096)) throw NativeError();
        if (!GetDiskFreeSpace(volume, out uint sectors, out uint sectorBytes, out _, out _)) throw NativeError();
        if (!GetDiskFreeSpaceEx(volume, out ulong available, out ulong total, out ulong free)) throw NativeError();
        if (!GetVolumeInformation(volume, null, 0, out uint serial, out uint maxName, out uint flags, null, 0)) throw NativeError();
        ulong block = (ulong)sectors * sectorBytes;
        if (block == 0) throw Failure(5);
        // Windows does not expose a POSIX inode pool. Those fields have no
        // meaningful value here (as permitted for filesystem-specific fields).
        return new PosixStatvfs {
            f_bsize = block, f_frsize = block, f_blocks = total / block,
            f_bfree = free / block, f_bavail = available / block,
            f_files = 0, f_ffree = 0, f_favail = 0, f_fsid = serial,
            f_flag = 2UL | ((flags & 0x80000) != 0 ? 1UL : 0UL), f_namemax = maxName,
        };
    }

    internal static PosixWinsize TerminalSize(SafeFileHandle handle)
    {
        EnsureSupported();
        if (OperatingSystem.IsWindows())
        {
            if (!GetConsoleMode(handle, out _)) throw Failure(25);
            // Input console handles also denote the process console; query its
            // output screen buffer even when stdout was redirected elsewhere.
            SafeFileHandle screen;
            fixed (char* name = "CONOUT$") screen = CreateFile(name, 0x80000000, 3, 0, 3, 0, 0);
            using (screen)
            {
                if (screen.IsInvalid || !GetConsoleScreenBufferInfo(screen, out ConsoleScreenBufferInfo info)) throw Failure(25);
                return new PosixWinsize {
                    ws_col = checked((ushort)(info.Right - info.Left + 1)),
                    ws_row = checked((ushort)(info.Bottom - info.Top + 1)),
                };
            }
        }
        ushort* record = stackalloc ushort[4];
        int fd = (int)handle.DangerousGetHandle();
        bool darwin = OperatingSystem.IsMacOS();
        int result = darwin && RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? TerminalIoctlDarwin(fd, 0x40087468, 0, 0, 0, 0, 0, 0, record)
            : TerminalIoctlLinux(fd, darwin ? 0x40087468UL : 0x5413UL, record);
        if (result < 0) throw NativeError();
        return new PosixWinsize { ws_row = record[0], ws_col = record[1], ws_xpixel = record[2], ws_ypixel = record[3] };
    }

    [StructLayout(LayoutKind.Sequential)] private struct ConsoleScreenBufferInfo
    {
        internal short SizeX, SizeY, CursorX, CursorY;
        internal ushort Attributes;
        internal short Left, Top, Right, Bottom, MaximumX, MaximumY;
    }
    [LibraryImport("libc", EntryPoint = "statvfs", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)] private static partial int StatvfsNative(string path, byte* record);
    [LibraryImport("libc", EntryPoint = "ioctl", SetLastError = true)] private static partial int TerminalIoctlLinux(int fd, ulong request, void* record);
    [LibraryImport("libc", EntryPoint = "ioctl", SetLastError = true)] private static partial int TerminalIoctlDarwin(int fd, ulong request, nint a, nint b, nint c, nint d, nint e, nint f, void* record);
    [LibraryImport("kernel32.dll", EntryPoint = "GetVolumePathNameW", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetVolumePathName(char* path, char* volume, uint length);
    [LibraryImport("kernel32.dll", EntryPoint = "GetDiskFreeSpaceW", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetDiskFreeSpace(char* volume, out uint sectors, out uint sectorBytes, out uint freeClusters, out uint totalClusters);
    [LibraryImport("kernel32.dll", EntryPoint = "GetDiskFreeSpaceExW", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetDiskFreeSpaceEx(char* volume, out ulong available, out ulong total, out ulong free);
    [LibraryImport("kernel32.dll", EntryPoint = "GetVolumeInformationW", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetVolumeInformation(char* volume, char* label, uint labelLength, out uint serial, out uint maxName, out uint flags, char* filesystem, uint filesystemLength);
    [LibraryImport("kernel32.dll", EntryPoint = "GetConsoleMode", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetConsoleMode(SafeFileHandle handle, out uint mode);
    [LibraryImport("kernel32.dll", EntryPoint = "GetConsoleScreenBufferInfo", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static partial bool GetConsoleScreenBufferInfo(SafeFileHandle handle, out ConsoleScreenBufferInfo info);
}
