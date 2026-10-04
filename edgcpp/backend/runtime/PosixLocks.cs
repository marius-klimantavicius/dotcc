#nullable enable
using System;
using Microsoft.Win32.SafeHandles;

namespace Edg.Runtime;

// Each translated owner is a virtual POSIX process. Unix uses kernel OFD locks
// so unrelated CLR/native descriptor closes cannot release its locks. Windows
// uses the shared advisory registry because kernel locks lack atomic conversion.
internal static class PosixLocks
{
    internal static void Control(object owner, SafeFileHandle handle, string path, int command, ref PosixFlock record, Func<bool>? closed = null)
    {
        if (command is not (5 or 6 or 7) || record.l_type is not (0 or 1 or 2) || record.l_whence != 0 || command == 5 && record.l_type == 2)
            throw PosixPlatform.Failure(22);
        long start = record.l_start, end;
        try
        {
            if (record.l_len < 0) { end = start; start = checked(start + record.l_len); }
            else end = record.l_len == 0 ? long.MaxValue : checked(start + record.l_len);
        }
        catch (OverflowException) { throw PosixPlatform.Failure(75); }
        if (start < 0 || end <= start) throw PosixPlatform.Failure(22);
        var normalized = record;
        normalized.l_start = start; normalized.l_len = end == long.MaxValue ? 0 : end - start;
        if (OperatingSystem.IsWindows()) PosixWindowsLocks.Control(owner, handle, path, command, ref normalized, closed);
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()) PosixOfdLocks.Control(owner, handle, path, command, ref normalized, closed);
        else throw PosixPlatform.Failure(95);
        if (command == 5) record = normalized;
    }
    internal static void Close(object owner, SafeFileHandle handle, string path)
    {
        if (OperatingSystem.IsWindows()) PosixWindowsLocks.Close(owner, handle, path);
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()) PosixOfdLocks.Close(owner, handle, path);
        else { handle.Dispose(); throw PosixPlatform.Failure(95); }
    }
}

public sealed unsafe partial class PosixContext
{
    public int Fcntl(int fd, int command, PosixFlock* record)
    {
        if (record == null) { SetError(14); return -1; }
        var descriptor = Find(fd); if (descriptor is null) return -1;
        try
        {
            PosixFlock request = *record;
            lock (descriptor.Gate)
            {
                if (descriptor.Closed) throw PosixPlatform.Failure(9);
                if (command != 5 && (request.l_type == 0 && (descriptor.Flags & 3) == 1 || request.l_type == 1 && (descriptor.Flags & 3) == 0)) throw PosixPlatform.Failure(9);
                long origin = request.l_whence switch { 0 => 0, 1 => descriptor.Position, 2 => PosixPlatform.Fstat(descriptor.Handle, descriptor.Path).st_size, _ => throw PosixPlatform.Failure(22) };
                request.l_start = checked(origin + request.l_start); request.l_whence = 0;
            }
            PosixLocks.Control(LockOwner, descriptor.Handle, descriptor.Path, command, ref request, () => descriptor.Closed);
            if (command == 5) *record = request; return 0;
        }
        catch (Exception error) { return Fail(error); }
    }
}
