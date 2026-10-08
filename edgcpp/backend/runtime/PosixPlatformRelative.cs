#nullable enable
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Edg.Runtime;

internal static unsafe partial class PosixPlatform
{
    internal static SafeFileHandle OpenAt(SafeFileHandle directory, string name, int flags, int mode)
    {
        ValidateOpenFlags(flags);
        if (OperatingSystem.IsWindows())
        {
            uint access = (flags & 3) switch { 0 => 0x120089u, 1 => 0x120116u, _ => 0x12019fu };
            uint disposition = (flags & 64) != 0 ? ((flags & 128) != 0 ? 2u : (flags & 512) != 0 ? 5u : 3u) : (flags & 512) != 0 ? 4u : 1u;
            uint options = 0x20u | ((flags & 131072) != 0 ? 0x200000u : 0u) |
                ((flags & (1048576 | 4096)) != 0 ? 2u : 0u) | ((flags & 65536) != 0 ? 1u : 0u);
            var handle = OpenRelativeWindows(directory, name, access, disposition, options);
            try
            {
                uint attributes = WindowsInfo(handle).Attributes;
                if ((flags & 131072) != 0 && (attributes & 0x400) != 0) throw Failure(40);
                if ((flags & 65536) != 0 && (attributes & 16) == 0) throw Failure(20);
                return handle;
            }
            catch { handle.Dispose(); throw; }
        }
        byte[] bytes = Encoding.UTF8.GetBytes(name + "\0");
        int fd, nativeFlags = UnixOpenFlags(flags);
        fixed (byte* path = bytes)
            do { fd = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? OpenAtDarwin(directory, path, nativeFlags, 0, 0, 0, 0, 0, (uint)mode)
                : OpenAtUnix(directory, path, nativeFlags, (uint)mode); }
            while (fd < 0 && Marshal.GetLastPInvokeError() == 4);
        if (fd < 0) throw NativeError();
        return new SafeFileHandle(fd, true);
    }

    internal static void UnlinkAt(SafeFileHandle directory, string name, bool removeDirectory)
    {
        EnsureSupported();
        if (OperatingSystem.IsWindows())
        {
            using var handle = OpenRelativeWindows(directory, name, 0x110080, 1, 0x200020);
            DeleteWindowsHandle(handle, removeDirectory);
            return;
        }
        int nativeFlags = removeDirectory ? (OperatingSystem.IsMacOS() ? 0x80 : 0x200) : 0;
        if (UnlinkAtUnix(directory, name, nativeFlags) != 0) throw NativeError();
    }

    internal static void UnlinkWindows(string name, bool removeDirectory)
    {
        SafeFileHandle handle;
        fixed (char* path = name) handle = CreateFile(path, 0x10080, 7, 0, 3, 0x02200000, 0);
        using (handle)
        {
            if (handle.IsInvalid) throw NativeError();
            DeleteWindowsHandle(handle, removeDirectory);
        }
    }

    private static void DeleteWindowsHandle(SafeFileHandle handle, bool removeDirectory)
    {
        uint attributes = WindowsInfo(handle).Attributes;
        bool isDirectory = (attributes & 16) != 0 && (attributes & 0x400) == 0;
        if (removeDirectory && !isDirectory) throw Failure(20);
        if (!removeDirectory && isDirectory) throw Failure(21);
        uint flags = 1 | 2 | 16; // DELETE | POSIX_SEMANTICS | IGNORE_READONLY_ATTRIBUTE
        if (!SetDispositionEx(handle, 21, ref flags, 4)) throw NativeError();
    }

    internal static void HardLink(string existing, string created)
    {
        EnsureSupported();
        if (OperatingSystem.IsWindows())
        {
            if (!CreateHardLink(created, existing, 0)) throw NativeError();
        }
        else if (LinkUnix(existing, created) != 0) throw NativeError();
    }

    // Relative object names are resolved by the kernel against RootDirectory.
    // Do not reconstruct a pathname: the directory may already have been renamed.
    private static SafeFileHandle OpenRelativeWindows(SafeFileHandle directory, string name, uint access, uint disposition, uint options)
    {
        name = name.Replace('/', '\\');
        if (name.Length == 0) throw Failure(2);
        string[] parts = name.Split('\\');
        if (Array.IndexOf(parts, ".") < 0 && Array.IndexOf(parts, "..") < 0)
            return OpenRelativeWindowsRaw(directory, name, access, disposition, options);
        SafeFileHandle? owned = null;
        try
        {
            for (int i = 0; i < parts.Length; ++i)
            {
                string part = parts[i];
                bool last = i == parts.Length - 1;
                SafeFileHandle current = owned ?? directory;
                if (part == "..")
                {
                    var parent = WindowsParentDirectory(current);
                    owned?.Dispose(); owned = parent;
                    if (last) return OpenRelativeWindowsRaw(parent, "", access, disposition, options);
                }
                else if (part is "." or "")
                {
                    if (last) return OpenRelativeWindowsRaw(current, "", access, disposition, options);
                }
                else if (last) return OpenRelativeWindowsRaw(current, part, access, disposition, options);
                else
                {
                    var next = OpenRelativeWindowsRaw(current, part, 0x100080, 1, 0x21);
                    owned?.Dispose(); owned = next;
                }
            }
            throw Failure(2);
        }
        finally { owned?.Dispose(); }
    }

    private static SafeFileHandle WindowsParentDirectory(SafeFileHandle directory)
    {
        // NT object names do not interpret "..". Resolve a current handle path,
        // open its parent, then prove the original directory is still in that
        // parent by file identity. Retry concurrent moves rather than relying on
        // the historical path stored when the portable fd was opened.
        var identity = LockIdentity(directory, "");
        char* buffer = stackalloc char[4096];
        for (int attempt = 0; attempt < 8; ++attempt)
        {
            uint length = GetFinalPathName(directory, buffer, 4096, 0);
            if (length == 0) throw NativeError();
            if (length >= 4096) throw Failure(36);
            string path = new(buffer, 0, (int)length);
            string? parentPath = Path.GetDirectoryName(path.TrimEnd('\\'));
            if (parentPath == null || path.TrimEnd('\\') == Path.GetPathRoot(path)?.TrimEnd('\\'))
                return OpenRelativeWindowsRaw(directory, "", 0x100080, 1, 0x21);
            SafeFileHandle parent;
            fixed (char* name = parentPath) parent = CreateFile(name, 0x100080, 7, 0, 3, 0x02000000, 0);
            if (parent.IsInvalid) { parent.Dispose(); continue; }
            try
            {
                using var child = OpenRelativeWindowsRaw(parent, Path.GetFileName(path.TrimEnd('\\')), 0x100080, 1, 0x21);
                if (LockIdentity(child, "") == identity) return parent;
            }
            catch (Win32Exception error) when (error.NativeErrorCode is 2 or 3) { }
            catch { parent.Dispose(); throw; }
            parent.Dispose();
        }
        throw Failure(11);
    }

    private static SafeFileHandle OpenRelativeWindowsRaw(SafeFileHandle directory, string name, uint access, uint disposition, uint options)
    {
        if (name.Length > 32766) throw Failure(36);
        bool held = false;
        directory.DangerousAddRef(ref held);
        try
        {
            fixed (char* text = name)
            {
                UnicodeString value = new() { Length = (ushort)(name.Length * 2), MaximumLength = (ushort)(name.Length * 2), Buffer = text };
                ObjectAttributes attributes = new() { Length = (uint)sizeof(ObjectAttributes), RootDirectory = directory.DangerousGetHandle(), Name = &value, Attributes = 0x40 };
                int status = NtCreateFile(out nint file, access, &attributes, out _, null, 0x80, 7, disposition, options, null, 0);
                if (status < 0) throw NtError(status);
                return new SafeFileHandle(file, true);
            }
        }
        finally { if (held) directory.DangerousRelease(); }
    }
    private static Exception NtError(int status) => new Win32Exception((int)RtlNtStatusToDosError(status));

    // The stream owns a duplicate of the descriptor, not its historical path.
    // POSIX fdopendir ownership of the original portable fd is handled by Context.
    internal sealed class DirectoryStream : IDisposable
    {
        private SafeDirectoryHandle? unixStream;
        private SafeFileHandle? windowsHandle;
        private bool first = true, finished;
        internal DirectoryStream(SafeFileHandle descriptor)
        {
            EnsureSupported();
            if (OperatingSystem.IsWindows())
            {
                if (!DuplicateDirectoryHandle(-1, descriptor, -1, out var duplicate, 0, false, 2)) throw NativeError();
                windowsHandle = duplicate;
            }
            else
            {
                int command = OperatingSystem.IsMacOS() ? 67 : 1030; // F_DUPFD_CLOEXEC
                int duplicate = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                    ? FcntlDarwin(descriptor, command, 0, 0, 0, 0, 0, 0, null)
                    : FcntlUnix(descriptor, command, null);
                if (duplicate < 0) throw NativeError();
                nint stream = FdopendirUnix(duplicate);
                if (stream == 0)
                {
                    var error = NativeError();
                    using var failed = new SafeFileHandle(duplicate, true);
                    throw error;
                }
                try { unixStream = new SafeDirectoryHandle(stream); }
                catch { ClosedirUnix(stream); throw; }
            }
        }
        internal string? Read()
        {
            if (finished) return null;
            byte* buffer = stackalloc byte[4096];
            for (;;)
            {
                string name;
                if (OperatingSystem.IsWindows())
                {
                    // FILE_NAMES_INFORMATION: NextEntryOffset, FileIndex,
                    // FileNameLength, then counted UTF-16 data. One entry per call.
                    int status = NtQueryDirectoryFile(windowsHandle!, 0, 0, 0, out IoStatusBlock io,
                        buffer, 4096, 12, 1, null, first ? (byte)1 : (byte)0);
                    first = false;
                    if (status == unchecked((int)0x80000006)) { finished = true; return null; }
                    if (status < 0) throw NtError(status);
                    uint length = *(uint*)(buffer + 8);
                    if (io.Information < 12 || length > io.Information - 12 || (length & 1) != 0) throw Failure(5);
                    name = new string((char*)(buffer + 12), 0, checked((int)length / 2));
                }
                else
                {
                    byte* entry = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.X64
                        ? ReaddirDarwin(unixStream!) : ReaddirUnix(unixStream!);
                    if (entry == null)
                    {
                        int error = Marshal.GetLastPInvokeError();
                        if (error != 0) throw new Win32Exception(error);
                        finished = true; return null;
                    }
                    // Linux dirent64 and Darwin's 64-bit inode dirent have
                    // different name offsets; neither layout crosses our ABI.
                    int offset = OperatingSystem.IsMacOS() ? 21 : 19;
                    name = Marshal.PtrToStringUTF8((nint)(entry + offset))!;
                }
                if (name is not ("." or "..")) return name;
            }
        }
        public void Dispose()
        {
            unixStream?.Dispose(); unixStream = null;
            windowsHandle?.Dispose(); windowsHandle = null; finished = true;
        }
    }

    private sealed class SafeDirectoryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeDirectoryHandle(nint value) : base(true) { SetHandle(value); }
        protected override bool ReleaseHandle() => ClosedirUnix(handle) == 0;
    }

    [StructLayout(LayoutKind.Sequential)] private struct UnicodeString { internal ushort Length, MaximumLength; internal char* Buffer; }
    [StructLayout(LayoutKind.Sequential)] private struct ObjectAttributes { internal uint Length; internal nint RootDirectory; internal UnicodeString* Name; internal uint Attributes; internal nint SecurityDescriptor, SecurityQuality; }
    [StructLayout(LayoutKind.Sequential)] private struct IoStatusBlock { internal nint Status; internal nuint Information; }
    [LibraryImport("ntdll.dll")] private static partial int NtCreateFile(out nint file, uint access, ObjectAttributes* attributes, out IoStatusBlock status, long* allocation, uint fileAttributes, uint share, uint disposition, uint options, void* extendedAttributes, uint extendedLength);
    [LibraryImport("ntdll.dll")] private static partial int NtQueryDirectoryFile(SafeFileHandle file, nint eventHandle, nint apcRoutine, nint apcContext, out IoStatusBlock status, void* buffer, uint length, int informationClass, byte singleEntry, UnicodeString* name, byte restart);
    [LibraryImport("ntdll.dll")] private static partial uint RtlNtStatusToDosError(int status);
    [LibraryImport("kernel32.dll", EntryPoint = "SetFileInformationByHandle", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool SetDispositionEx(SafeFileHandle file, int informationClass, ref uint flags, uint length);
    [LibraryImport("kernel32.dll", EntryPoint = "DuplicateHandle", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool DuplicateDirectoryHandle(nint sourceProcess, SafeFileHandle source, nint targetProcess, out SafeFileHandle target, uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint options);
    [LibraryImport("kernel32.dll", EntryPoint = "CreateHardLinkW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static partial bool CreateHardLink(string created, string existing, nint security);
    [LibraryImport("libc", EntryPoint = "openat", SetLastError = true)] private static partial int OpenAtUnix(SafeFileHandle directory, byte* name, int flags, uint mode);
    [LibraryImport("libc", EntryPoint = "openat", SetLastError = true)] private static partial int OpenAtDarwin(SafeFileHandle directory, byte* name, int flags, nint a, nint b, nint c, nint d, nint e, uint mode);
    [LibraryImport("libc", EntryPoint = "unlinkat", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)] private static partial int UnlinkAtUnix(SafeFileHandle directory, string name, int flags);
    [LibraryImport("libc", EntryPoint = "link", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)] private static partial int LinkUnix(string existing, string created);
    [LibraryImport("libc", EntryPoint = "fdopendir", SetLastError = true)] private static partial nint FdopendirUnix(int descriptor);
    [LibraryImport("libc", EntryPoint = "readdir", SetLastError = true)] private static partial byte* ReaddirUnix(SafeDirectoryHandle directory);
    [LibraryImport("libc", EntryPoint = "readdir$INODE64", SetLastError = true)] private static partial byte* ReaddirDarwin(SafeDirectoryHandle directory);
    [LibraryImport("libc", EntryPoint = "closedir", SetLastError = true)] private static partial int ClosedirUnix(nint directory);
}
