#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Edg.Runtime;

// File.Delete deliberately suppresses missing-path errors, and separate BCL
// file/directory rename branches cannot implement one atomic rename without a
// type-check race. These small runtime-selected operations preserve the native
// filesystem result; stream opening and ordinary byte I/O remain BCL services.
internal static unsafe partial class LibcPaths
{
    internal static void Remove(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            fixed (char* name = path)
            {
                if (DeleteFile(name) != 0) return;
                int firstError = Marshal.GetLastPInvokeError();
                if (firstError == 5)
                {
                    if (RemoveDirectory(name) != 0) return;
                    int directoryError = Marshal.GetLastPInvokeError();
                    if (directoryError != 267) firstError = directoryError;
                }
                throw Error(firstError);
            }
        }
        RequireUnix();
        byte[] bytes = Encoding.UTF8.GetBytes(path + "\0");
        fixed (byte* name = bytes) if (RemoveUnix(name) != 0) throw Error(Marshal.GetLastPInvokeError());
    }

    internal static void Rename(string source, string destination)
    {
        if (OperatingSystem.IsWindows())
        {
            fixed (char* from = source, to = destination)
                if (MoveFile(from, to, 1) == 0) throw Error(Marshal.GetLastPInvokeError()); // REPLACE_EXISTING, not COPY_ALLOWED
            return;
        }
        RequireUnix();
        byte[] oldBytes = Encoding.UTF8.GetBytes(source + "\0"), newBytes = Encoding.UTF8.GetBytes(destination + "\0");
        fixed (byte* from = oldBytes, to = newBytes)
            if (RenameUnix(from, to) != 0) throw Error(Marshal.GetLastPInvokeError());
    }
    private static void RequireUnix()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("Filesystem mutation is implemented for Windows, Linux and macOS.");
    }
    private static IOException Error(int error) => new("Filesystem operation failed (platform error " + error + ").", error);
    [LibraryImport("libc", EntryPoint = "remove", SetLastError = true)]
    private static partial int RemoveUnix(byte* path);
    [LibraryImport("libc", EntryPoint = "rename", SetLastError = true)]
    private static partial int RenameUnix(byte* source, byte* destination);
    [LibraryImport("kernel32.dll", EntryPoint = "DeleteFileW", SetLastError = true)]
    private static partial int DeleteFile(char* path);
    [LibraryImport("kernel32.dll", EntryPoint = "RemoveDirectoryW", SetLastError = true)]
    private static partial int RemoveDirectory(char* path);
    [LibraryImport("kernel32.dll", EntryPoint = "MoveFileExW", SetLastError = true)]
    private static partial int MoveFile(char* source, char* destination, uint flags);
}
