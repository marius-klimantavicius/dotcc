#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Linux statx with owner-local descriptors and the kernel's full
    /// 256-byte UAPI output. No managed approximation of struct statx is used.
    /// Managed consoles/pipes without native handles and non-Linux LP64 hosts
    /// explicitly fail with ENOTSUP; unavailable libc statx fails with ENOSYS.</summary>
    public static int statx(int descriptor, byte* path, uint flags, uint mask, void* output)
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) { errno = ENOTSUP; return -1; }
        // Linux validates the request before looking up dirfd/path. Preserve
        // EINVAL/ENOENT precedence even when a guest fd is invalid.
        if ((flags & ~0x7900u) != 0 || (flags & 0x6000u) == 0x6000u || (mask & 0x80000000u) != 0)
        { errno = EINVAL; return -1; }
        if (path == null) { errno = EFAULT; return -1; }
        if (*path == 0 && (flags & 0x1000u) == 0) { errno = ENOENT; return -1; }
        SafeHandle? handle = null;
        bool retained = false;
        try
        {
            const int atCwd = -100;
            // Absolute paths ignore dirfd, including an otherwise invalid fd.
            if (*path != '/' && descriptor != atCwd)
            {
                lock (_filesLock)
                {
                    var slot = SlotByFd(descriptor);
                    if (slot is null) { errno = EBADF; return -1; }
                    handle = slot.Stream switch
                    {
                        FileStream file => file.SafeFileHandle,
                        BackendDirectoryStream directory => directory.Handle,
                        _ => slot.Socket?.SafeHandle,
                    };
                    if (handle is null) { errno = ENOTSUP; return -1; }
                    handle.DangerousAddRef(ref retained);
                }
                descriptor = handle.DangerousGetHandle().ToInt32();
            }
            // RuntimeContext cwd is independent of the process cwd. Prefix
            // bytes without decoding/normalizing the caller's Linux pathname.
            // EMPTY_PATH with AT_FDCWD describes that same owner-local cwd.
            if (descriptor == atCwd && *path != '/' && RuntimeContext.Current is not null
                && (*path != 0 || (flags & 0x1000) != 0))
            {
                byte[] prefix = Encoding.UTF8.GetBytes(CurrentWorkingDirectory + (*path == 0 ? "" : "/"));
                int length = checked((int)strlen(path));
                byte[] resolved = new byte[checked(prefix.Length + length + 1)];
                prefix.CopyTo(resolved, 0);
                new ReadOnlySpan<byte>(path, length).CopyTo(resolved.AsSpan(prefix.Length));
                fixed (byte* name = resolved) return LinuxStatxResult(HostLinuxStatx(atCwd, name, flags, mask, output));
            }
            return LinuxStatxResult(HostLinuxStatx(descriptor, path, flags, mask, output));
        }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
        catch (EntryPointNotFoundException) { errno = ENOSYS; return -1; }
        catch (DllNotFoundException) { errno = ENOSYS; return -1; }
        catch (OutOfMemoryException) { errno = ENOMEM; return -1; }
        catch (OverflowException) { errno = ENAMETOOLONG; return -1; }
        finally { if (retained) handle!.DangerousRelease(); }
    }

    private static int LinuxStatxResult(int result)
    {
        if (result < 0) errno = Marshal.GetLastPInvokeError();
        return result;
    }

    /// <summary>Zig std.os.linux returns raw negative errno as source usize,
    /// unlike libc's -1 and thread-local errno. Preserve libc errno here.</summary>
    public static ulong __dotcc_linux_statx(int descriptor, byte* path, uint flags, uint mask, void* output)
    {
        int saved = errno;
        try { return statx(descriptor, path, flags, mask, output) == 0 ? 0 : unchecked((ulong)-(long)errno); }
        finally { errno = saved; }
    }

    [DllImport("libc", EntryPoint = "statx", SetLastError = true)]
    private static extern int HostLinuxStatx(int descriptor, byte* path, uint flags, uint mask, void* output);
}
