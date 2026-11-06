#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public static int* __dotcc_zig_c___errno_location() => RuntimeThread.ErrnoAddress;
    public static long __dotcc_zig_c_lseek64(int descriptor, long offset, int whence)
    {
        var slot = SlotByFd(descriptor);
        if (slot is null) { errno = EBADF; return -1; }
        // The managed seek service supplies SET/CUR/END, which are the source
        // I/O wrapper's reached operations. Do not reinterpret DATA/HOLE as SET.
        if (whence is 3 or 4) { errno = ENOTSUP; return -1; }
        if (whence is < 0 or > 2) { errno = EINVAL; return -1; }
        try
        {
            if (slot.Stream is { CanSeek: true } stream)
            {
                Int128 position = (Int128)(whence == 0 ? 0 : whence == 1 ? stream.Position : stream.Length) + offset;
                if (position < 0) { errno = EINVAL; return -1; }
                if (position > long.MaxValue) { errno = EOVERFLOW; return -1; }
            }
            return lseek(descriptor, offset, whence);
        }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
        catch (ArgumentException) { errno = EINVAL; return -1; }
        catch (OverflowException) { errno = EOVERFLOW; return -1; }
    }
    public static long __dotcc_zig_c_preadv64(int descriptor, void* vectors, uint count, long offset) =>
        preadv(descriptor, vectors, count > int.MaxValue ? -1 : (int)count, offset);
    public static void* __dotcc_zig_c_mmap64(void* address, ulong length, uint protection, uint flags, int descriptor, long offset) =>
        mmap(address, length, unchecked((int)protection), unchecked((int)flags), descriptor, offset);

    // Borrow the guest directory's owning handle while the existing path
    // services operate through procfs. A renamed directory retains its open-file
    // identity; no synthetic descriptor is ever passed to an OS operation.
    private static byte[]? SourceAtPath(int descriptor, byte* path, out SafeHandle? handle, out bool retained)
    {
        handle = null; retained = false;
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
        { errno = ENOTSUP; return null; }
        if (path == null) { errno = EFAULT; return null; }
        if (*path == 0) { errno = ENOENT; return null; }
        if (*path == '/' || descriptor == -100) return Encoding.UTF8.GetBytes(ResolvePath(path) + "\0");
        lock (_filesLock)
        {
            var slot = SlotByFd(descriptor);
            if (slot is null) { errno = EBADF; return null; }
            handle = slot.Stream switch
            {
                FileStream file => file.SafeFileHandle,
                BackendDirectoryStream directory => directory.Handle,
                _ => null,
            };
            if (handle is null) { errno = ENOTDIR; return null; }
            handle.DangerousAddRef(ref retained);
        }
        return Encoding.UTF8.GetBytes("/proc/self/fd/" + handle.DangerousGetHandle().ToInt64()
            .ToString(global::System.Globalization.CultureInfo.InvariantCulture) + "/" + Str(path) + "\0");
    }

    public static int __dotcc_zig_c_openat64(int descriptor, byte* path, uint flags, uint mode)
    {
        SafeHandle? handle = null; bool retained = false;
        try
        {
            byte[]? resolved = SourceAtPath(descriptor, path, out handle, out retained);
            if (resolved is null) return -1;
            fixed (byte* name = resolved) return open(name, unchecked((int)flags), unchecked((int)mode));
        }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
        catch (OutOfMemoryException) { errno = ENOMEM; return -1; }
        finally { if (retained) handle!.DangerousRelease(); }
    }

    public static int __dotcc_zig_c_unlinkat(int descriptor, byte* path, uint flags)
    {
        if ((flags & ~0x200U) != 0) { errno = EINVAL; return -1; }
        SafeHandle? handle = null; bool retained = false;
        try
        {
            byte[]? resolved = SourceAtPath(descriptor, path, out handle, out retained);
            if (resolved is null) return -1;
            fixed (byte* name = resolved)
            {
                int result = SourceUnlinkAt(-100, name, flags);
                if (result < 0) errno = Marshal.GetLastPInvokeError();
                return result;
            }
        }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
        catch (OutOfMemoryException) { errno = ENOMEM; return -1; }
        finally { if (retained) handle!.DangerousRelease(); }
    }

    public static ulong __dotcc_zig_c_malloc_usable_size(void* pointer)
    {
        if (pointer == null) return 0;
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("Source malloc_usable_size requires Linux x64 libc");
        lock (ownedHeapLock)
        {
            bool owned = FindOwnedHeapBlock(pointer, "inspect usable size of", out var block);
            if (owned && block.Kind == HeapAllocationKind.Debug)
                return (ulong)DbgValidate((byte*)pointer, "malloc_usable_size", out _);
            lock (_alignedLock)
                if (_alignedBlocks.TryGetValue((nuint)pointer, out var aligned))
                {
                    ValidateAlignedBlock((nuint)pointer, aligned, "malloc_usable_size");
                    if (aligned.Debug) return (ulong)aligned.Size;
                    return SourceMallocUsableSize((void*)aligned.Base) - ((ulong)pointer - (ulong)aligned.Base);
                }
            if (!owned && _dbgHeap) return (ulong)DbgValidate((byte*)pointer, "malloc_usable_size", out _);
            return SourceMallocUsableSize(pointer);
        }
    }

    [DllImport("libc", EntryPoint = "malloc_usable_size")]
    private static extern ulong SourceMallocUsableSize(void* pointer);
    [DllImport("libc", EntryPoint = "unlinkat", SetLastError = true)]
    private static extern int SourceUnlinkAt(int descriptor, byte* path, uint flags);
}
